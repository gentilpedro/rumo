namespace EnadeAds.Study;

/// <summary>Nível da prova: diz para quem a trilha é, e não muda o funcionamento.</summary>
public enum TrackLevel
{
    /// <summary>Seletivos para entrar no ensino médio (ex.: técnico integrado dos IFs), com conteúdo do fundamental.</summary>
    ElementarySchool = 0,
    HighSchool = 1,
    HigherEducation = 2,

    /// <summary>Concursos públicos: o nível de escolaridade exigido vem no nome da trilha.</summary>
    PublicExam = 3,
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

    /// <summary>Cota de questões por matéria fixada no edital (concursos e vestibulares), em <see cref="MockExams.SubjectExam"/>.</summary>
    Subjects = 3,
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
    public const int IbgeRecenseador = 7;
    public const int Ufsm1 = 8;
    public const int Ufsm2 = 9;
    public const int Ufsm3 = 10;

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
        // Edital IBGE nº 03/2026 (Cesgranrio), itens 3, 10.1 e 10.6: 27.279 vagas de nível fundamental, prova em 31/01/2027.
        new()
        {
            Id = IbgeRecenseador, Slug = "ibge-recenseador", Name = "IBGE 2027 · Recenseador", ExamName = "IBGE", Label = "IBGE Recenseador",
            Basis = "O conteúdo do Edital IBGE nº 03/2026 para Recenseador do 12º Censo Agro: Português, Matemática e a apostila oficial de Conhecimentos Técnicos.",
            Level = TrackLevel.PublicExam, MockExamStyle = MockExamStyle.Subjects,
            ExamDate = new(2027, 1, 31), Order = 7,
        },
        // UFSM: vestibular e Processo Seletivo Seriado usam as mesmas provas (manual do candidato 2027, itens 2.1 e 9.4.1).
        new()
        {
            Id = Ufsm1, Slug = "ufsm-1", Name = "UFSM 2027 · Prova 1 (PSS1)", ExamName = "UFSM", Label = "UFSM Prova 1",
            Basis = "As habilidades do 1º ano do ensino médio no Caderno de Habilidades da UFSM, que valem para a Prova 1 do vestibular e para o PSS1.",
            Level = TrackLevel.HighSchool, MockExamStyle = MockExamStyle.Subjects,
            ExamDate = new(2027, 1, 9), Order = 8,
        },
        new()
        {
            Id = Ufsm2, Slug = "ufsm-2", Name = "UFSM 2027 · Prova 2 (PSS2)", ExamName = "UFSM", Label = "UFSM Prova 2",
            Basis = "As habilidades do 2º ano do ensino médio no Caderno de Habilidades da UFSM, que valem para a Prova 2 do vestibular e para o PSS2.",
            Level = TrackLevel.HighSchool, MockExamStyle = MockExamStyle.Subjects,
            ExamDate = new(2027, 1, 10), Order = 9,
        },
        new()
        {
            Id = Ufsm3, Slug = "ufsm-3", Name = "UFSM 2027 · Prova 3 (PSS3)", ExamName = "UFSM", Label = "UFSM Prova 3",
            Basis = "As habilidades do 3º ano do ensino médio e a redação no Caderno de Habilidades da UFSM, que valem para a Prova 3 do vestibular e para o PSS3.",
            Level = TrackLevel.HighSchool, MockExamStyle = MockExamStyle.Subjects,
            ExamDate = new(2027, 1, 10), Order = 10,
        },
    ];

    public static int? IdOf(string? slug) => Seed.FirstOrDefault(t => t.Slug == slug)?.Id;

    public static string LevelName(this TrackLevel level) => level switch
    {
        TrackLevel.ElementarySchool => "Ensino fundamental",
        TrackLevel.HighSchool => "Ensino médio",
        TrackLevel.HigherEducation => "Ensino superior",
        TrackLevel.PublicExam => "Concurso público",
        _ => level.ToString(),
    };
}
