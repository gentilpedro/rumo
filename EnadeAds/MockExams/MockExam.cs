using EnadeAds.Study;

namespace EnadeAds.MockExams;

public enum MockExamSize
{
    Short = 0,
    Medium = 1,
    Full = 2,
}

/// <summary>Simulado de um aluno. O prazo é gravado no início: recarregar a página não reinicia o relógio.</summary>
public class MockExam
{
    public int Id { get; set; }
    public int TrackId { get; set; } = Study.Tracks.Default;
    public required string UserId { get; set; }
    public MockExamSize Size { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset DeadlineAt { get; set; }

    /// <summary>Preenchido na entrega, pelo aluno ou automaticamente quando o tempo acaba.</summary>
    public DateTimeOffset? SubmittedAt { get; set; }

    public bool SubmittedByTimeout { get; set; }
    public int ObjectiveCount { get; set; }
    public int? CorrectCount { get; set; }

    /// <summary>PAVE: quantas questões o aluno entregou com "Ignoro a resposta". Não somam nem descontam.</summary>
    public int IgnoredCount { get; set; }

    /// <summary>Erradas e em branco: no PAVE, as duas descontam.</summary>
    public int WrongCount => ObjectiveCount - (CorrectCount ?? 0) - IgnoredCount;

    public List<MockExamQuestion> Questions { get; set; } = [];

    public bool IsOpen => SubmittedAt is null;
}

public class MockExamQuestion
{
    public int MockExamId { get; set; }
    public int QuestionId { get; set; }
    public Questions.Question? Question { get; set; }
    public int Order { get; set; }

    /// <summary>Formação geral ou componente específico, como na prova.</summary>
    public bool IsGeneralEducation { get; set; }

    public int? SelectedOptionId { get; set; }

    /// <summary>PAVE: o aluno marcou a alternativa "f", "Ignoro a resposta". Exclui <see cref="SelectedOptionId"/>.</summary>
    public bool IgnoredAnswer { get; set; }

    public string? DiscursiveText { get; set; }
}

/// <summary>
/// Quantas questões de cada parte o simulado pede, conforme o estilo da trilha.
/// ENADE (Portarias Inep nº 169 e 172/2026, ADS e Redes): 15 de formação geral + 30 específicas + 1 discursiva em 4 horas.
/// ENEM: 90 questões objetivas num dia de prova de 5 horas (o segundo dia), equilibradas entre as áreas;
/// a redação não entra no simulado. No ENEM, <see cref="Specific"/> são as objetivas.
/// PAVE (Edital CRA/UFPel nº 16/2026): 32 objetivas em 3h30, com cota por área (<see cref="PaveQuotas"/>);
/// a redação da Etapa 3 não entra no simulado. No PAVE, <see cref="Specific"/> também são as objetivas.
/// </summary>
public sealed record MockExamFormat(MockExamStyle Style, MockExamSize Size, string Name, int GeneralEducation, int Specific, int Discursive)
{
    public static IReadOnlyList<MockExamFormat> All { get; } =
    [
        new(MockExamStyle.Enade, MockExamSize.Short, "Curto", 3, 7, 1),
        new(MockExamStyle.Enade, MockExamSize.Medium, "Médio", 7, 15, 1),
        new(MockExamStyle.Enade, MockExamSize.Full, "Completo, como a prova", 15, 30, 1),
        new(MockExamStyle.Enem, MockExamSize.Short, "Curto", 0, 20, 0),
        new(MockExamStyle.Enem, MockExamSize.Medium, "Médio", 0, 45, 0),
        new(MockExamStyle.Enem, MockExamSize.Full, "Completo, como um dia de prova", 0, 90, 0),
        new(MockExamStyle.Pave, MockExamSize.Short, "Curto", 0, 11, 0),
        new(MockExamStyle.Pave, MockExamSize.Medium, "Médio", 0, 21, 0),
        new(MockExamStyle.Pave, MockExamSize.Full, "Completo, como a prova", 0, 32, 0),
    ];

    public int Total => GeneralEducation + Specific + Discursive;

    public static IReadOnlyList<MockExamFormat> For(MockExamStyle style) => All.Where(f => f.Style == style).ToList();

    public static MockExamFormat Of(MockExamStyle style, MockExamSize size) => All.Single(f => f.Style == style && f.Size == size);

    /// <summary>Formato de um simulado já feito, pela trilha dele.</summary>
    public static MockExamFormat Of(MockExam exam) => Of(StyleOf(exam.TrackId), exam.Size);

    public static MockExamStyle StyleOf(int trackId) =>
        Tracks.Seed.FirstOrDefault(t => t.Id == trackId)?.MockExamStyle ?? MockExamStyle.Enade;

    /// <summary>
    /// Tempo proporcional ao da prova: 4 horas para 46 questões no ENADE, 5 horas para 90 no ENEM e
    /// 3h30 para 32 no PAVE.
    /// </summary>
    public static int MinutesFor(MockExamStyle style, int questionCount)
    {
        var (minutes, questions) = style switch
        {
            MockExamStyle.Enem => (300, 90),
            MockExamStyle.Pave => (210, 32),
            _ => (240, 46),
        };
        return Math.Max(10, (int)Math.Round(minutes * questionCount / (double)questions));
    }

    /// <summary>
    /// PAVE: quantas questões de cada parte, na ordem da prova. A prova completa tem 9 de Ciências da Natureza,
    /// 9 de Ciências Humanas, 5 de Matemática e 9 de Linguagens, das quais as 3 últimas são de língua estrangeira
    /// (item 12.5 e 12.6.2 do edital); os tamanhos menores guardam a proporção.
    /// </summary>
    public static IReadOnlyList<PaveQuota> PaveQuotas(MockExamSize size) => size switch
    {
        MockExamSize.Short => Quotas(3, 3, 2, 2, 1),
        MockExamSize.Medium => Quotas(6, 6, 3, 4, 2),
        _ => Quotas(9, 9, 5, 6, 3),
    };

    private static PaveQuota[] Quotas(int nature, int humanities, int mathematics, int languages, int foreignLanguage) =>
    [
        new(PavePart.NaturalSciences, nature),
        new(PavePart.Humanities, humanities),
        new(PavePart.Mathematics, mathematics),
        new(PavePart.Languages, languages),
        new(PavePart.ForeignLanguage, foreignLanguage),
    ];
}

/// <summary>Partes da prova objetiva do PAVE, na ordem em que aparecem.</summary>
public enum PavePart
{
    NaturalSciences,
    Humanities,
    Mathematics,
    Languages,
    ForeignLanguage,
}

public sealed record PaveQuota(PavePart Part, int Count);

/// <summary>
/// Nota do PAVE (Edital CRA/UFPel nº 16/2026, itens 12.4, 13 e 14): cada erro, ou questão em branco, desconta
/// 10% do valor de um acerto; "Ignoro a resposta" não soma nem desconta. Nas Etapas 1 e 2 o acerto vale
/// 3,125 pontos (100 nas 32 questões); na Etapa 3 vale 2,6875 (86), e a redação completa os outros 14.
/// </summary>
public static class PaveScoring
{
    public const decimal WrongAnswerPenalty = 0.1m;
    public const decimal EssayMaxPoints = 14m;

    public static decimal PointsPerQuestion(int trackId) => trackId == Tracks.Pave3 ? 2.6875m : 3.125m;

    public static decimal Score(int trackId, int correct, int wrong) =>
        PointsPerQuestion(trackId) * (correct - WrongAnswerPenalty * wrong);

    public static decimal Score(MockExam exam) => Score(exam.TrackId, exam.CorrectCount ?? 0, exam.WrongCount);

    /// <summary>Nota de quem acerta tudo, para o número de questões do simulado.</summary>
    public static decimal MaxScore(MockExam exam) => PointsPerQuestion(exam.TrackId) * exam.ObjectiveCount;

    /// <summary>"53,13": vírgula decimal, até duas casas, sem zeros sobrando.</summary>
    public static string Format(decimal points) => points.ToString("0.##", Culture);

    private static readonly System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
}
