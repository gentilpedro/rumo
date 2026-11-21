using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.Reviews;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Questions;

public sealed class OptionDraft
{
    public string Text { get; set; } = "";
    public string Explanation { get; set; } = "";
}

public sealed class QuestionDraft
{
    /// <summary>Máximo de alternativas, e quantos campos a tela mostra. ENADE, ENEM e PAVE usam cinco.</summary>
    public const int OptionCount = 5;

    /// <summary>Algumas provas têm quatro alternativas (OAB, IFSul): a quinta fica em branco e não é gravada.</summary>
    public const int MinOptionCount = 4;

    public QuestionKind Kind { get; set; }
    public int SubjectId { get; set; }
    public string Statement { get; set; } = "";
    public string Source { get; set; } = "";
    public char CorrectLetter { get; set; }
    public List<OptionDraft> Options { get; set; } = Enumerable.Range(0, OptionCount).Select(_ => new OptionDraft()).ToList();
    public string ExpectedAnswer { get; set; } = "";
    public int? SuggestedLines { get; set; }

    public static QuestionDraft From(Question question) => new()
    {
        Kind = question.Kind,
        SubjectId = question.SubjectId,
        ExpectedAnswer = question.ExpectedAnswer ?? "",
        SuggestedLines = question.SuggestedLines,
        Statement = question.Statement,
        Source = question.Source,
        CorrectLetter = question.Options.FirstOrDefault(o => o.IsCorrect)?.Letter ?? 'A',
        Options = question.Kind == QuestionKind.Objective
            ? question.Options.OrderBy(o => o.Letter).Select(o => new OptionDraft { Text = o.Text, Explanation = o.Explanation }).ToList()
            : new QuestionDraft().Options,
    };
}

/// <summary>
/// Quem está editando: o id da conta e se ela pode mexer em questões de outros (Admin).
/// <paramref name="Reviewed"/> marca uma alteração que outro professor ou um admin já aprovou (ver <see cref="Reviews.ReviewDecisions"/>).
/// </summary>
public sealed record Editor(string UserId, bool IsAdmin, bool IsTeacher, bool Reviewed = false)
{
    public bool CanCreate => IsTeacher || IsAdmin;

    public bool CanEdit(Question question) => IsAdmin || (IsTeacher && question.AuthorId == UserId);

    /// <summary>Professor (sem ser admin) não publica direto: a alteração vai para a fila de revisão.</summary>
    public bool NeedsReview => IsTeacher && !IsAdmin && !Reviewed;
}

/// <summary>Resultado de uma edição. Com <see cref="QueuedReviewId"/>, a alteração foi para revisão e ainda não vale.</summary>
public sealed record EditResult(int? Id, IReadOnlyList<string> Errors)
{
    public int? QueuedReviewId { get; init; }

    public bool Succeeded => Errors.Count == 0;

    public bool IsQueued => QueuedReviewId is not null;

    public static EditResult Fail(params string[] errors) => new(null, errors);

    public static EditResult Queued(int reviewId) => new(null, []) { QueuedReviewId = reviewId };
}

public sealed class QuestionEditor(ApplicationDbContext db, ReviewQueue reviews, TimeProvider clock, ILogger<QuestionEditor> logger)
{
    public const int StatementMaxLength = 12000;
    public const int SourceMaxLength = 200;
    public const int OptionMaxLength = 2000;
    public const int ExpectedAnswerMaxLength = 5000;
    public const int MaxSuggestedLines = 60;

    public async Task<EditResult> CreateAsync(Editor editor, QuestionDraft draft, CancellationToken cancellationToken = default)
    {
        if (!editor.CanCreate)
        {
            return EditResult.Fail("Só professores e administradores publicam questões.");
        }

        var errors = await ValidateAsync(draft, cancellationToken);
        if (errors.Count > 0)
        {
            return new EditResult(null, errors);
        }

        if (editor.NeedsReview)
        {
            return await reviews.SubmitAsync(editor, ReviewKind.QuestionCreate, null, draft, draft.Statement, cancellationToken);
        }

        var objective = draft.Kind == QuestionKind.Objective;
        var question = new Question
        {
            Kind = draft.Kind,
            SubjectId = draft.SubjectId,
            Statement = Normalize(draft.Statement),
            Source = draft.Source.Trim(),
            AuthorId = editor.UserId,
            CreatedAt = clock.GetUtcNow(),
            ExpectedAnswer = objective ? null : Normalize(draft.ExpectedAnswer),
            SuggestedLines = objective ? null : draft.SuggestedLines,
            Options = objective ? draft.Options.Select((option, index) => NewOption(draft, option, index)).ToList() : [],
        };

        db.Questions.Add(question);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Questão {QuestionId} criada por {UserId}.", question.Id, editor.UserId);

        return new EditResult(question.Id, []);
    }

    public async Task<EditResult> UpdateAsync(Editor editor, int questionId, QuestionDraft draft, CancellationToken cancellationToken = default)
    {
        var question = await db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
        if (question is null)
        {
            return EditResult.Fail("Questão não encontrada.");
        }

        if (!editor.CanEdit(question))
        {
            return EditResult.Fail("Só o autor da questão ou um administrador pode editá-la.");
        }

        // O tipo fica fixo depois de publicada: as respostas já dadas dependem dele.
        if (draft.Kind != question.Kind)
        {
            return EditResult.Fail("O tipo da questão (objetiva ou discursiva) não muda depois de publicada.");
        }

        var errors = await ValidateAsync(draft, cancellationToken);
        if (errors.Count > 0)
        {
            return new EditResult(questionId, errors);
        }

        // As tentativas dos alunos apontam para as alternativas: dá para mudar o texto, não a quantidade.
        if (question.Kind == QuestionKind.Objective && draft.Options.Count != question.Options.Count)
        {
            return EditResult.Fail($"Esta questão tem {question.Options.Count} alternativas, e o número não muda depois de publicada.");
        }

        if (editor.NeedsReview)
        {
            return await reviews.SubmitAsync(editor, ReviewKind.QuestionUpdate, questionId, draft, draft.Statement, cancellationToken);
        }

        question.SubjectId = draft.SubjectId;
        question.Statement = Normalize(draft.Statement);
        question.Source = draft.Source.Trim();

        if (question.Kind == QuestionKind.Discursive)
        {
            question.ExpectedAnswer = Normalize(draft.ExpectedAnswer);
            question.SuggestedLines = draft.SuggestedLines;
        }

        // Atualiza as alternativas no lugar (mesmo id por letra), para as tentativas antigas continuarem apontando para elas.
        foreach (var (option, index) in question.Options.OrderBy(o => o.Letter).Select((o, i) => (o, i)))
        {
            var updated = NewOption(draft, draft.Options[index], index);
            option.Text = updated.Text;
            option.Explanation = updated.Explanation;
            option.IsCorrect = updated.IsCorrect;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Questão {QuestionId} editada por {UserId}.", question.Id, editor.UserId);

        return new EditResult(question.Id, []);
    }

    public async Task<EditResult> ArchiveAsync(Editor editor, int questionId, CancellationToken cancellationToken = default)
    {
        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
        if (question is null)
        {
            return EditResult.Fail("Questão não encontrada.");
        }

        if (!editor.CanEdit(question))
        {
            return EditResult.Fail("Só o autor da questão ou um administrador pode arquivá-la.");
        }

        if (editor.NeedsReview)
        {
            return await reviews.SubmitAsync(editor, ReviewKind.QuestionArchive, questionId, null, question.Statement, cancellationToken);
        }

        question.ArchivedAt ??= clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Questão {QuestionId} arquivada por {UserId}.", question.Id, editor.UserId);

        return new EditResult(question.Id, []);
    }

    private async Task<List<string>> ValidateAsync(QuestionDraft draft, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var subjectExists = await db.RoadmapNodes.AnyAsync(n => n.Id == draft.SubjectId && n.Kind == NodeKind.Subject, cancellationToken);
        if (!subjectExists)
        {
            errors.Add("Escolha a matéria da questão.");
        }

        Require(draft.Statement, "o enunciado", StatementMaxLength, errors);
        Require(draft.Source, "a fonte", SourceMaxLength, errors);

        if (draft.Kind == QuestionKind.Discursive)
        {
            Require(draft.ExpectedAnswer, "o padrão de resposta", ExpectedAnswerMaxLength, errors);
            if (draft.SuggestedLines is < 1 or > MaxSuggestedLines)
            {
                errors.Add($"O tamanho sugerido precisa ficar entre 1 e {MaxSuggestedLines} linhas.");
            }

            return errors;
        }

        if (draft.Kind != QuestionKind.Objective)
        {
            errors.Add("Escolha se a questão é objetiva ou discursiva.");
            return errors;
        }

        // Questão de quatro alternativas: a quinta veio em branco da tela e sai daqui.
        if (draft.Options.Count == QuestionDraft.OptionCount && string.IsNullOrWhiteSpace(draft.Options[^1].Text)
            && string.IsNullOrWhiteSpace(draft.Options[^1].Explanation))
        {
            draft.Options = draft.Options.Take(QuestionDraft.MinOptionCount).ToList();
        }

        if (draft.Options.Count is < QuestionDraft.MinOptionCount or > QuestionDraft.OptionCount)
        {
            errors.Add($"A questão precisa de {QuestionDraft.MinOptionCount} ou {QuestionDraft.OptionCount} alternativas.");
            return errors;
        }

        for (var index = 0; index < draft.Options.Count; index++)
        {
            var letter = (char)('A' + index);
            Require(draft.Options[index].Text, $"o texto da alternativa {letter}", OptionMaxLength, errors);
            Require(draft.Options[index].Explanation, $"o comentário da alternativa {letter}", OptionMaxLength, errors);
        }

        if (draft.CorrectLetter < 'A' || draft.CorrectLetter >= 'A' + draft.Options.Count)
        {
            errors.Add("Marque qual alternativa é a correta.");
        }

        return errors;
    }

    private static void Require(string? value, string field, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Preencha {field}.");
        }
        else if (value.Trim().Length > maxLength)
        {
            errors.Add($"Encurte {field}: o limite é {maxLength} caracteres.");
        }
    }

    private static QuestionOption NewOption(QuestionDraft draft, OptionDraft option, int index)
    {
        var letter = (char)('A' + index);
        return new QuestionOption
        {
            Letter = letter,
            Text = option.Text.Trim(),
            Explanation = option.Explanation.Trim(),
            IsCorrect = letter == draft.CorrectLetter,
        };
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n").Trim();
}

public static class EditorFactory
{
    public static Editor? From(System.Security.Claims.ClaimsPrincipal user, string? userId) =>
        userId is null ? null : new Editor(userId, user.IsInRole(Roles.Admin), user.IsInRole(Roles.Teacher));
}
