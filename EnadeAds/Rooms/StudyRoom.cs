namespace EnadeAds.Rooms;

/// <summary>
/// Sala de estudos: um grupo fechado de colegas da mesma prova. Só entra quem recebe o link, que carrega o
/// <see cref="Code"/> e não o id, para ninguém achar salas alheias contando números. Não há busca de pessoas,
/// lista pública de salas nem conversa: a plataforma aceita menores de idade (LGPD, art. 14).
/// </summary>
public class StudyRoom
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public int TrackId { get; set; }

    /// <summary>O único texto livre da sala. Um membro pode denunciar; a moderação troca o nome ou apaga a sala.</summary>
    public required string Name { get; set; }

    /// <summary>Nulo se o dono excluiu a conta: no próximo acesso a sala passa para o membro mais antigo.</summary>
    public string? OwnerId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<StudyRoomMember> Members { get; set; } = [];
}

/// <summary>
/// Participação numa sala. Guarda só o mínimo: quem, em qual sala e desde quando. O <see cref="Id"/> identifica
/// o membro nos formulários do dono, para o id da conta não ir para a página dos outros.
/// </summary>
public class StudyRoomMember
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}

/// <summary>Denúncia de uma sala (do nome dela), feita por um membro.</summary>
public class StudyRoomReport
{
    public long Id { get; set; }
    public int RoomId { get; set; }

    /// <summary>Nulo quando quem denunciou excluiu a conta.</summary>
    public string? ReporterId { get; set; }

    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolvedById { get; set; }
}
