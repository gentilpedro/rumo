namespace EnadeAds.Duels;

/// <summary>
/// Duelo: as mesmas questões objetivas para quem abrir o link. Ganha quem acertar mais; no empate, quem terminou
/// mais rápido. O link carrega o <see cref="Code"/>, não o id, para ninguém achar duelos alheios contando números.
/// </summary>
public class Duel
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public int TrackId { get; set; }

    /// <summary>Nulo se quem criou excluiu a conta: o placar dos outros fica.</summary>
    public string? CreatorId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<DuelQuestion> Questions { get; set; } = [];
    public List<DuelPlayer> Players { get; set; } = [];

    /// <summary>Até quando dá para entrar no duelo.</summary>
    public DateTimeOffset JoinUntil => CreatedAt + DuelService.JoinWindow;
}

public class DuelQuestion
{
    public int DuelId { get; set; }
    public int QuestionId { get; set; }
    public Questions.Question? Question { get; set; }
    public int Order { get; set; }
}

/// <summary>Participação de uma pessoa: o relógio começa em <see cref="StartedAt"/> e vale para ela só.</summary>
public class DuelPlayer
{
    public int DuelId { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Preenchido no envio das respostas.</summary>
    public DateTimeOffset? FinishedAt { get; set; }

    public int? CorrectCount { get; set; }

    public List<DuelAnswer> Answers { get; set; } = [];

    public bool HasFinished => FinishedAt is not null;

    /// <summary>Tempo de jogo, limitado ao prazo: quem enviou depois não ganha nem perde por isso.</summary>
    public TimeSpan? Elapsed => FinishedAt is { } finished
        ? TimeSpan.FromTicks(Math.Min((finished - StartedAt).Ticks, DuelService.TimeLimit.Ticks))
        : null;
}

public class DuelAnswer
{
    public int DuelId { get; set; }
    public required string UserId { get; set; }
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
    public bool IsCorrect { get; set; }
}
