namespace EnadeAds.Achievements;

/// <summary>Quanto vale cada coisa. Ficam juntos para a tabela da página e o cálculo não divergirem.</summary>
public static class XpRules
{
    /// <summary>Primeira resposta a uma questão objetiva, certa ou errada.</summary>
    public const int FirstAnswer = 2;

    /// <summary>Primeiro acerto de uma questão. Acertar de novo não conta: XP não sai de repetir a mesma questão.</summary>
    public const int FirstCorrect = 10;

    public const int Discursive = 15;
    public const int MockExam = 25;

    /// <summary>A mais por entregar o simulado completo, do tamanho da prova.</summary>
    public const int FullMockExamBonus = 25;

    public const int Challenge = 20;
    public const int Duel = 10;

    /// <summary>Vencer um duelo em que pelo menos duas pessoas enviaram.</summary>
    public const int DuelWin = 15;

    public const int RoadmapTopic = 5;

    /// <summary>XP de responder uma questão agora, sabendo o que a pessoa já tinha feito nela.</summary>
    public static int ForAnswer(bool answeredBefore, bool correctBefore, bool isCorrect) =>
        (answeredBefore ? 0 : FirstAnswer) + (isCorrect && !correctBefore ? FirstCorrect : 0);
}

/// <summary>Nível pelo XP acumulado. Cada nível pede 50 XP a mais que o anterior: 0, 50, 150, 300, 500...</summary>
public sealed record Level(int Number, string Title, int Floor, int? NextAt)
{
    private static readonly string[] Titles =
        ["Calouro", "Aprendiz", "Estudante", "Dedicado", "Focado", "Veterano", "Expert", "Mestre", "Lenda"];

    public static int FloorOf(int number) => 25 * number * (number - 1);

    public static Level Of(int xp)
    {
        var number = 1;
        while (number < Titles.Length && xp >= FloorOf(number + 1))
        {
            number++;
        }

        int? next = number < Titles.Length ? FloorOf(number + 1) : null;
        return new Level(number, Titles[number - 1], FloorOf(number), next);
    }

    /// <summary>Quanto do caminho até o próximo nível já foi feito, de 0 a 100.</summary>
    public int PercentTo(int xp) => NextAt is { } next ? (int)Math.Floor(100.0 * (xp - Floor) / (next - Floor)) : 100;
}
