namespace EnadeAds.Performance;

/// <summary>
/// Estrelas de uma matéria pelo acerto da pessoa nela, como fases de um jogo: 1 a partir de 40%, 2 a partir de 60% e
/// 3 a partir de 80%. Só valem depois de <see cref="MinimumAnswered"/> questões (ou todas, se a matéria tiver menos):
/// acertar 1 de 1 não é dominar a matéria.
/// </summary>
public static class Stars
{
    public const int Max = 3;
    public const int MinimumAnswered = 5;

    private static readonly int[] Thresholds = [40, 60, 80];

    /// <summary>De 0 a 3; nulo enquanto a pessoa respondeu poucas questões da matéria.</summary>
    public static int? Of(SubjectScore subject) =>
        subject.TotalQuestions == 0 || subject.Score.Answered < Needed(subject)
            ? null
            : Thresholds.Count(threshold => subject.Score.Percent >= threshold);

    /// <summary>Quantas questões da matéria a pessoa precisa ter respondido para ganhar estrelas.</summary>
    public static int Needed(SubjectScore subject) => Math.Min(MinimumAnswered, subject.TotalQuestions);

    /// <summary>Acerto necessário para a próxima estrela; nulo com as 3.</summary>
    public static int? NextAt(int stars) => stars < Max ? Thresholds[stars] : null;
}
