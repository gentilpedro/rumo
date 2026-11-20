using System.Text.Json;
using System.Text.Json.Serialization;
using EnadeAds.Data;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Questions;

/// <summary>
/// Importa as questões das provas do Inep a partir dos arquivos JSON em Data/Seed/inep. É idempotente: cada questão
/// tem uma chave (ex.: enade-2021-ads-q12) e só entra se a chave ainda não existe. Questões já importadas nunca são
/// sobrescritas, para não desfazer correções que os professores fizerem pela tela.
/// </summary>
public sealed class InepQuestionImporter(ApplicationDbContext db, ILogger<InepQuestionImporter> logger)
{
    public static readonly DateTimeOffset ImportDate = new(2026, 10, 30, 0, 0, 0, TimeSpan.Zero);

    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };

    /// <summary>Pastas lidas na subida: provas do Inep (ENADE ADS e Redes), questões de autoria própria do ENEM e provas do PAVE (UFPel).</summary>
    public static IReadOnlyList<string> DefaultFolders { get; } =
    [
        Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "inep"),
        Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "redes"),
        Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "enem"),
        Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "pave"),
    ];

    public static string DefaultFolder => DefaultFolders[0];

    public async Task<int> ImportFolderAsync(string folder, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(folder))
        {
            return 0;
        }

        var imported = 0;
        foreach (var file in Directory.EnumerateFiles(folder, "*.json").Order())
        {
            SeedExam? exam;
            try
            {
                await using var stream = File.OpenRead(file);
                exam = await JsonSerializer.DeserializeAsync<SeedExam>(stream, JsonOptions, cancellationToken);
            }
            catch (JsonException error)
            {
                // Arquivo malformado não impede a aplicação de subir: fica de fora e aparece no log.
                logger.LogError(error, "Arquivo de questões {File} inválido; nenhuma questão dele foi importada.", Path.GetFileName(file));
                continue;
            }

            var trackId = Tracks.IdOf(exam?.Track ?? "enade-ads");
            if (trackId is null)
            {
                logger.LogError("Arquivo de questões {File} aponta para a trilha {Track}, que não existe.", Path.GetFileName(file), exam?.Track);
                continue;
            }

            imported += await ImportAsync(exam?.Questions ?? [], Path.GetFileName(file), trackId.Value, cancellationToken);

            // Formação Geral é a mesma prova para todos os cursos: o arquivo vale também para as outras trilhas do ENADE.
            foreach (var slug in exam?.SharedWith ?? [])
            {
                var sharedTrackId = Tracks.IdOf(slug);
                if (sharedTrackId is null)
                {
                    logger.LogError("Arquivo de questões {File} é compartilhado com a trilha {Track}, que não existe.", Path.GetFileName(file), slug);
                    continue;
                }

                var copies = exam!.Questions.Select(q => q with { Key = SharedKey(q.Key, slug) }).ToList();
                imported += await ImportAsync(copies, Path.GetFileName(file), sharedTrackId.Value, cancellationToken);
            }
        }

        if (imported > 0)
        {
            logger.LogInformation("{Count} questões importadas de {Folder}.", imported, Path.GetFileName(folder));
        }

        return imported;
    }

    public Task<int> ImportAsync(IReadOnlyList<SeedQuestion> questions, string origin, CancellationToken cancellationToken = default) =>
        ImportAsync(questions, origin, Tracks.EnadeAds, cancellationToken);

    /// <summary>As matérias (<c>subject</c>) são procuradas só na trilha <paramref name="trackId"/>; a chave é única na plataforma.</summary>
    public async Task<int> ImportAsync(IReadOnlyList<SeedQuestion> questions, string origin, int trackId, CancellationToken cancellationToken = default)
    {
        var existing = await db.Questions.IgnoreQueryFilters().Where(q => q.ExternalKey != null).Select(q => q.ExternalKey!).ToListAsync(cancellationToken);
        var known = existing.ToHashSet(StringComparer.Ordinal);
        var subjects = await db.RoadmapNodes.IgnoreQueryFilters()
            .Where(n => n.TrackId == trackId && n.Kind == NodeKind.Subject)
            .ToDictionaryAsync(n => n.Slug, n => n.Id, cancellationToken);

        var added = 0;
        foreach (var seed in questions)
        {
            if (known.Contains(seed.Key))
            {
                continue;
            }

            var problem = Validate(seed, subjects);
            if (problem is not null)
            {
                // Uma questão com defeito no arquivo não derruba a aplicação: fica de fora e aparece no log.
                logger.LogWarning("Questão {Key} de {File} ignorada: {Problem}", seed.Key, origin, problem);
                continue;
            }

            db.Questions.Add(ToQuestion(seed, subjects[seed.Subject]));
            known.Add(seed.Key);
            added++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return added;
    }

    /// <summary>Chave da cópia de uma questão compartilhada: cada trilha tem a sua questão, com respostas e correções próprias.</summary>
    public static string SharedKey(string key, string trackSlug) => $"{key}@{trackSlug}";

    public static string? Validate(SeedQuestion seed, IReadOnlyDictionary<string, int> subjects)
    {
        if (string.IsNullOrWhiteSpace(seed.Key) || seed.Key.Length > 60)
        {
            return "chave vazia ou longa demais";
        }

        if (!subjects.ContainsKey(seed.Subject))
        {
            return $"matéria desconhecida: {seed.Subject}";
        }

        if (string.IsNullOrWhiteSpace(seed.Statement) || seed.Statement.Length > QuestionEditor.StatementMaxLength)
        {
            return "enunciado vazio ou longo demais";
        }

        if (string.IsNullOrWhiteSpace(seed.Source) || seed.Source.Length > QuestionEditor.SourceMaxLength)
        {
            return "fonte vazia ou longa demais";
        }

        return seed.Kind switch
        {
            QuestionKind.Objective when seed.Options is not { Count: QuestionDraft.OptionCount } => "a objetiva precisa de 5 alternativas",
            QuestionKind.Objective when seed.Correct is not { Length: 1 } || seed.Correct[0] is < 'A' or > 'E' => "alternativa correta inválida",
            QuestionKind.Objective when seed.Options!.Any(o => string.IsNullOrWhiteSpace(o.Text) || string.IsNullOrWhiteSpace(o.Explanation)
                || o.Text.Length > QuestionEditor.OptionMaxLength || o.Explanation.Length > QuestionEditor.OptionMaxLength) => "alternativa sem texto ou comentário",
            QuestionKind.Discursive when string.IsNullOrWhiteSpace(seed.ExpectedAnswer) || seed.ExpectedAnswer.Length > QuestionEditor.ExpectedAnswerMaxLength
                => "discursiva sem padrão de resposta",
            _ => null,
        };
    }

    private static Question ToQuestion(SeedQuestion seed, int subjectId)
    {
        var objective = seed.Kind == QuestionKind.Objective;
        return new Question
        {
            ExternalKey = seed.Key,
            Kind = seed.Kind,
            SubjectId = subjectId,
            Statement = seed.Statement.Replace("\r\n", "\n").Trim(),
            Source = seed.Source.Trim(),
            CreatedAt = ImportDate,
            ExpectedAnswer = objective ? null : seed.ExpectedAnswer!.Replace("\r\n", "\n").Trim(),
            SuggestedLines = objective ? null : seed.SuggestedLines ?? 15,
            Options = objective
                ? seed.Options!.Select((option, index) => new QuestionOption
                {
                    Letter = (char)('A' + index),
                    Text = option.Text.Trim(),
                    Explanation = option.Explanation.Trim(),
                    IsCorrect = (char)('A' + index) == seed.Correct![0],
                }).ToList()
                : [],
        };
    }
}

/// <param name="Track">Slug da trilha das questões; sem ele, ENADE ADS (os arquivos do Inep vieram antes das trilhas).</param>
/// <param name="SharedWith">Outras trilhas que recebem uma cópia das questões, como a Formação Geral do ENADE.</param>
public sealed record SeedExam(int Year, IReadOnlyList<SeedQuestion> Questions, string? Track = null, IReadOnlyList<string>? SharedWith = null);

public sealed record SeedQuestion(
    string Key,
    string Source,
    QuestionKind Kind,
    string Subject,
    string Statement,
    IReadOnlyList<SeedOption>? Options,
    string? Correct,
    string? ExpectedAnswer,
    int? SuggestedLines);

public sealed record SeedOption(string Text, string Explanation);
