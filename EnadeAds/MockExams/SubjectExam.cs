using EnadeAds.Study;

namespace EnadeAds.MockExams;

/// <summary>
/// Uma parte da prova pela cota do edital: quantas questões, de quais matérias (slugs do roadmap da trilha).
/// <see cref="IsForeignLanguage"/> marca a parte em que o candidato escolhe o idioma: ela usa só a matéria escolhida.
/// </summary>
public sealed record SubjectQuota(string Label, IReadOnlyList<string> Slugs, int Count, bool IsForeignLanguage = false);

/// <summary>
/// Prova cujo edital fixa quantas questões de cada matéria (concursos e vestibulares). O simulado completo segue a
/// cota; o curto e o médio guardam a proporção, com ao menos uma questão por parte. Nenhuma parte completa a falta de
/// outra, para o simulado não perder a proporção da prova.
/// </summary>
public sealed record SubjectExam(int TrackId, int FullMinutes, IReadOnlyList<SubjectQuota> Quotas)
{
    public int FullCount => Quotas.Sum(q => q.Count);

    public bool HasLanguageChoice => Quotas.Any(q => q.IsForeignLanguage);

    public IReadOnlyList<SubjectQuota> QuotasFor(MockExamSize size) => Quotas
        .Select(q => q with
        {
            Count = size switch
            {
                MockExamSize.Short => Math.Max(1, (int)Math.Round(q.Count / 3.0, MidpointRounding.AwayFromZero)),
                MockExamSize.Medium => Math.Max(1, (int)Math.Round(q.Count * 2 / 3.0, MidpointRounding.AwayFromZero)),
                _ => q.Count,
            },
        })
        .ToList();

    public int CountFor(MockExamSize size) => QuotasFor(size).Sum(q => q.Count);

    /// <summary>Tempo proporcional ao da prova, no mínimo 10 minutos.</summary>
    public int MinutesFor(int questionCount) => Math.Max(10, (int)Math.Round(FullMinutes * questionCount / (double)FullCount));

    /// <summary>"15 de Língua Portuguesa, 15 de Matemática e 30 de Conhecimentos Técnicos."</summary>
    public string Describe(MockExamSize size)
    {
        var parts = QuotasFor(size).Select(q => $"{q.Count} de {q.Label}").ToList();
        return parts.Count == 1 ? parts[0] + "." : string.Join(", ", parts[..^1]) + " e " + parts[^1] + ".";
    }

    /// <summary>Os idiomas entre os quais o candidato escolhe; o primeiro é o padrão.</summary>
    public static readonly IReadOnlyList<(string Slug, string Name)> Languages = [("ingles", "Inglês"), ("espanhol", "Espanhol")];

    public static IReadOnlyList<string> LanguageSlugs { get; } = Languages.Select(l => l.Slug).ToList();

    private static readonly Dictionary<int, SubjectExam> ByTrack = new[]
    {
        // Edital IBGE nº 03/2026, itens 10.2 e 10.6: 60 questões em 4 horas.
        new SubjectExam(Tracks.IbgeRecenseador, 240,
        [
            new("Língua Portuguesa", ["lingua-portuguesa"], 15),
            new("Matemática", ["matematica"], 15),
            new("Conhecimentos Técnicos", ["conhecimentos-tecnicos"], 30),
        ]),
        Ufsm(Tracks.Ufsm1),
        Ufsm(Tracks.Ufsm2),
        Ufsm(Tracks.Ufsm3),
    }.ToDictionary(e => e.TrackId);

    /// <summary>
    /// Manual do candidato UFSM 2027, itens 2.1, 9.1 e 9.3: 40 questões por prova, na mesma distribuição nas três, em
    /// 3 horas (a Prova 3 tem 5 horas porque inclui a redação, que não entra no simulado).
    /// </summary>
    private static SubjectExam Ufsm(int trackId) => new(trackId, 180,
    [
        new("Filosofia e Sociologia", ["filosofia-sociologia"], 2),
        new("Geografia", ["geografia"], 4),
        new("História", ["historia"], 4),
        new("Química", ["quimica"], 4),
        new("Física", ["fisica"], 4),
        new("Biologia", ["biologia"], 4),
        new("Matemática", ["matematica"], 5),
        new("Artes Integradas e Educação Física", ["artes-educacao-fisica"], 3),
        new("Língua Portuguesa", ["lingua-portuguesa"], 5),
        new("Literatura", ["literatura"], 2),
        new("Língua Estrangeira", LanguageSlugs, 3, IsForeignLanguage: true),
    ]);

    public static SubjectExam? Of(int trackId) => ByTrack.GetValueOrDefault(trackId);
}
