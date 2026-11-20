namespace EnadeAds.Study;

/// <summary>Nível da prova: diz para quem a trilha é, e não muda o funcionamento.</summary>
public enum TrackLevel
{
    /// <summary>Seletivos para entrar no ensino médio (ex.: técnico integrado dos IFs), com conteúdo do fundamental.</summary>
    ElementarySchool = 0,
    HighSchool = 1,
    HigherEducation = 2,
}

/// <summary>Como o simulado da trilha é montado (ver <see cref="MockExams.MockExamFormat"/>).</summary>
public enum MockExamStyle
{
    /// <summary>Formação geral + componente específico + discursiva (Portarias Inep nº 169 e 172/2026).</summary>
    Enade = 0,

    /// <summary>Questões objetivas distribuídas pelas quatro áreas, no ritmo de um dia de prova.</summary>
    Enem = 1,

    /// <summary>
    /// 32 objetivas com cota por área, a opção "Ignoro a resposta" e desconto de 10% por erro
    /// (Edital CRA/UFPel nº 16/2026, itens 12 e 13).
    /// </summary>
    Pave = 2,
}

/// <summary>
/// Trilha: uma prova-alvo numa edição (ENADE 2026 · ADS, ENEM 2026...). Roadmap, questões, desafios e
/// simulados pertencem a uma trilha; contas, moderação e o feed são da plataforma.
/// </summary>
public class Track
{
    public int Id { get; set; }
    public required string Slug { get; set; }

    /// <summary>Nome completo, com a edição: "ENADE 2026 · ADS".</summary>
    public required string Name { get; set; }

    /// <summary>Nome curto da prova, usado em frases como "dias até o ENEM".</summary>
    public required string ExamName { get; set; }

    /// <summary>O que aparece ao lado da marca Rumo: "ENEM", "ENADE ADS".</summary>
    public required string Label { get; set; }

    /// <summary>Uma frase sobre de onde vem o conteúdo da trilha (portaria, matriz, edital).</summary>
    public required string Basis { get; set; }

    public TrackLevel Level { get; set; }
    public MockExamStyle MockExamStyle { get; set; }

    /// <summary>Primeiro (ou único) dia de prova; a contagem regressiva vai até ele.</summary>
    public DateOnly ExamDate { get; set; }

    /// <summary>Segundo dia, para provas em dois domingos como o ENEM.</summary>
    public DateOnly? SecondExamDate { get; set; }

    public int Order { get; set; }

    public IEnumerable<DateOnly> ExamDays => SecondExamDate is { } second ? [ExamDate, second] : [ExamDate];

    /// <summary>Último dia de prova: a trilha só "acaba" depois dele.</summary>
    public DateOnly LastExamDay => SecondExamDate ?? ExamDate;
}

/// <summary>As trilhas da plataforma. Ids fixos porque roadmap e questões das seeds apontam para eles.</summary>
public static class Tracks
{
    public const int EnadeAds = 1;
    public const int Enem = 2;
    public const int EnadeRedes = 3;
    public const int Pave1 = 4;
    public const int Pave2 = 5;
    public const int Pave3 = 6;

    /// <summary>Trilha de quem ainda não escolheu nenhuma: a primeira que a plataforma teve.</summary>
    public const int Default = EnadeAds;

    public static IReadOnlyList<Track> Seed { get; } =
    [
        new()
        {
            Id = EnadeAds, Slug = "enade-ads", Name = "ENADE 2026 · ADS", ExamName = "ENADE", Label = "ENADE ADS",
            Basis = "Os conteúdos de Análise e Desenvolvimento de Sistemas da Portaria Inep nº 169/2026 e a Formação Geral, na ordem sugerida de estudo.",
            Level = TrackLevel.HigherEducation, MockExamStyle = MockExamStyle.Enade,
            // Edital Inep nº 49/2026.
            ExamDate = new(2026, 11, 29), Order = 1,
        },
        new()
        {
            Id = Enem, Slug = "enem", Name = "ENEM 2026", ExamName = "ENEM", Label = "ENEM",
            Basis = "As quatro áreas e a redação da Matriz de Referência do ENEM, na ordem dos dias de prova.",
            Level = TrackLevel.HighSchool, MockExamStyle = MockExamStyle.Enem,
            // Linguagens, Ciências Humanas e Redação no primeiro domingo; Natureza e Matemática no segundo.
            ExamDate = new(2026, 11, 8), SecondExamDate = new(2026, 11, 15), Order = 3,
        },
        new()
        {
            Id = EnadeRedes, Slug = "enade-redes", Name = "ENADE 2026 · Redes de Computadores", ExamName = "ENADE", Label = "ENADE Redes",
            Basis = "Os conteúdos de Tecnologia em Redes de Computadores da Portaria Inep nº 172/2026 e a Formação Geral, na ordem sugerida de estudo.",
            Level = TrackLevel.HigherEducation, MockExamStyle = MockExamStyle.Enade,
            // Mesmo dia do ENADE de ADS (Edital Inep nº 49/2026); as duas trilhas do ENADE ficam juntas na escolha.
            ExamDate = new(2026, 11, 29), Order = 2,
        },
        // PAVE (UFPel): uma etapa por ano do ensino médio. Datas do Edital CRA nº 16/2026, item 11.1.
        new()
        {
            Id = Pave1, Slug = "pave-1", Name = "PAVE 2026 · Etapa 1", ExamName = "PAVE", Label = "PAVE 1",
            Basis = "Os conteúdos do 1º ano do ensino médio na Matriz de Referência PAVE 2026 da UFPel, na ordem das áreas da prova.",
            Level = TrackLevel.HighSchool, MockExamStyle = MockExamStyle.Pave,
            ExamDate = new(2026, 12, 5), Order = 4,
        },
        new()
        {
            Id = Pave2, Slug = "pave-2", Name = "PAVE 2026 · Etapa 2", ExamName = "PAVE", Label = "PAVE 2",
            Basis = "Os conteúdos do 2º ano do ensino médio na Matriz de Referência PAVE 2026 da UFPel, na ordem das áreas da prova.",
            Level = TrackLevel.HighSchool, MockExamStyle = MockExamStyle.Pave,
            ExamDate = new(2026, 12, 6), Order = 5,
        },
        new()
        {
            Id = Pave3, Slug = "pave-3", Name = "PAVE 2026 · Etapa 3", ExamName = "PAVE", Label = "PAVE 3",
            Basis = "Os conteúdos do 3º ano do ensino médio e a redação na Matriz de Referência PAVE 2026 da UFPel, na ordem das áreas da prova.",
            Level = TrackLevel.HighSchool, MockExamStyle = MockExamStyle.Pave,
            ExamDate = new(2026, 12, 6), Order = 6,
        },
    ];

    public static int? IdOf(string? slug) => Seed.FirstOrDefault(t => t.Slug == slug)?.Id;

    public static string LevelName(this TrackLevel level) => level switch
    {
        TrackLevel.ElementarySchool => "Ensino fundamental",
        TrackLevel.HighSchool => "Ensino médio",
        TrackLevel.HigherEducation => "Ensino superior",
        _ => level.ToString(),
    };
}
