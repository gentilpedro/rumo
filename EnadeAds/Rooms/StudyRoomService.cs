using System.Security.Cryptography;
using System.Text.RegularExpressions;
using EnadeAds.Data;
using EnadeAds.Duels;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Rooms;

public enum CreateRoomOutcome
{
    Created,
    InvalidName,
    TooManyRooms,
}

public sealed record CreateRoomResult(CreateRoomOutcome Outcome, string? Code);

public enum JoinRoomOutcome
{
    Joined,
    AlreadyMember,
    NotFound,
    Full,
    TooManyRooms,
}

public enum RoomActionOutcome
{
    Done,
    NotFound,

    /// <summary>A ação é só do dono (ou só de membro) e quem pediu não é.</summary>
    NotAllowed,
    InvalidName,
}

public enum RoomModeration
{
    /// <summary>Encerra as denúncias sem mexer na sala.</summary>
    Keep,

    /// <summary>Troca o nome por <see cref="StudyRoomService.NeutralName"/>.</summary>
    ResetName,
    Delete,
}

/// <summary>Um membro como os outros o veem: só o primeiro nome e a inicial do sobrenome.</summary>
public sealed record RoomMemberView(int MemberId, string Name, bool IsOwner, bool IsMe);

/// <summary>Uma sala na lista "Suas salas".</summary>
public sealed record RoomSummary(string Code, string Name, int TrackId, int Members, bool IsOwner);

/// <summary>As denúncias abertas de uma sala, para a moderação.</summary>
public sealed record RoomReportGroup(int RoomId, string RoomName, int TrackId, int Members, int Count, DateTimeOffset FirstReportedAt, IReadOnlyList<string> Reasons);

/// <summary>
/// Salas de estudo. A sala é da trilha em que foi criada, mas se abre pelo link em qualquer trilha, como o duelo.
/// </summary>
public sealed partial class StudyRoomService(ApplicationDbContext db, TimeProvider clock, ILogger<StudyRoomService> logger)
{
    public const int MaxMembers = 30;

    /// <summary>Salas que uma pessoa pode ter criado ao mesmo tempo.</summary>
    public const int MaxOwned = 5;

    /// <summary>Salas de que uma pessoa pode participar ao mesmo tempo, contando as que criou.</summary>
    public const int MaxMemberships = 10;

    public const int CodeLength = 8;
    public const int NameMinLength = 3;
    public const int NameMaxLength = 60;
    public const int ReasonMaxLength = 500;

    /// <summary>Nome que a moderação põe no lugar de um nome denunciado.</summary>
    public const string NeutralName = "Sala de estudos";

    // Sem 0/O, 1/I/L: o código pode ser lido em voz alta ou digitado.
    private const string CodeAlphabet = "abcdefghjkmnpqrstuvwxyz23456789";

    /// <summary>Cria a sala na trilha ativa, a prova que a pessoa está estudando, e já a põe como dona e membro.</summary>
    public async Task<CreateRoomResult> CreateAsync(string userId, string? name, CancellationToken cancellationToken = default)
    {
        if (NormalizeName(name) is not { } valid)
        {
            return new CreateRoomResult(CreateRoomOutcome.InvalidName, null);
        }

        var owned = await db.StudyRooms.CountAsync(r => r.OwnerId == userId, cancellationToken);
        var memberships = await db.StudyRoomMembers.CountAsync(m => m.UserId == userId, cancellationToken);
        if (owned >= MaxOwned || memberships >= MaxMemberships)
        {
            return new CreateRoomResult(CreateRoomOutcome.TooManyRooms, null);
        }

        var now = clock.GetUtcNow();
        var room = new StudyRoom
        {
            Code = NewCode(),
            TrackId = db.CurrentTrackId,
            Name = valid,
            OwnerId = userId,
            CreatedAt = now,
            Members = [new StudyRoomMember { UserId = userId, JoinedAt = now }],
        };

        db.StudyRooms.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Sala {RoomId} criada na trilha {TrackId}.", room.Id, room.TrackId);
        return new CreateRoomResult(CreateRoomOutcome.Created, room.Code);
    }

    /// <summary>
    /// Sala com os membros; nulo se o código não existir. Conserta a sala que ficou sem dono (conta excluída):
    /// ela passa para o membro mais antigo, e a que ficou vazia é apagada.
    /// </summary>
    public async Task<StudyRoom?> FindAsync(string code, CancellationToken cancellationToken = default)
    {
        var room = await db.StudyRooms.Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Code == Normalize(code), cancellationToken);
        if (room is null)
        {
            return null;
        }

        if (room.Members.Count == 0)
        {
            db.StudyRooms.Remove(room);
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (room.Members.All(m => m.UserId != room.OwnerId))
        {
            room.OwnerId = Oldest(room).UserId;
            await db.SaveChangesAsync(cancellationToken);
        }

        return room;
    }

    public async Task<JoinRoomOutcome> JoinAsync(string code, string userId, CancellationToken cancellationToken = default)
    {
        var room = await FindAsync(code, cancellationToken);
        if (room is null)
        {
            return JoinRoomOutcome.NotFound;
        }

        if (room.Members.Any(m => m.UserId == userId))
        {
            return JoinRoomOutcome.AlreadyMember;
        }

        if (room.Members.Count >= MaxMembers)
        {
            return JoinRoomOutcome.Full;
        }

        if (await db.StudyRoomMembers.CountAsync(m => m.UserId == userId, cancellationToken) >= MaxMemberships)
        {
            return JoinRoomOutcome.TooManyRooms;
        }

        room.Members.Add(new StudyRoomMember { UserId = userId, JoinedAt = clock.GetUtcNow() });
        await db.SaveChangesAsync(cancellationToken);
        return JoinRoomOutcome.Joined;
    }

    /// <summary>Sai da sala. Se era o dono, a sala passa para o membro mais antigo; se era o último, a sala é apagada.</summary>
    public async Task<RoomActionOutcome> LeaveAsync(string code, string userId, CancellationToken cancellationToken = default)
    {
        var room = await FindAsync(code, cancellationToken);
        if (room is null)
        {
            return RoomActionOutcome.NotFound;
        }

        if (room.Members.FirstOrDefault(m => m.UserId == userId) is not { } member)
        {
            return RoomActionOutcome.NotAllowed;
        }

        Remove(room, member);
        await db.SaveChangesAsync(cancellationToken);
        return RoomActionOutcome.Done;
    }

    /// <summary>
    /// Tira a pessoa de todas as salas, antes de a conta ser excluída: as salas que ela criou passam para o membro
    /// mais antigo, e as que ficariam vazias são apagadas na hora, junto com o nome que ela escreveu.
    /// </summary>
    public async Task LeaveAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        var rooms = await db.StudyRooms.Include(r => r.Members)
            .Where(r => r.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
        foreach (var room in rooms)
        {
            Remove(room, room.Members.First(m => m.UserId == userId));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RoomActionOutcome> RenameAsync(string code, string userId, string? name, CancellationToken cancellationToken = default)
    {
        var (room, outcome) = await OwnedAsync(code, userId, cancellationToken);
        if (room is null)
        {
            return outcome;
        }

        if (NormalizeName(name) is not { } valid)
        {
            return RoomActionOutcome.InvalidName;
        }

        room.Name = valid;
        await db.SaveChangesAsync(cancellationToken);
        return RoomActionOutcome.Done;
    }

    /// <summary>Gera um link novo e invalida o anterior. Devolve o código novo, ou nulo se quem pediu não é o dono.</summary>
    public async Task<string?> RenewCodeAsync(string code, string userId, CancellationToken cancellationToken = default)
    {
        var (room, _) = await OwnedAsync(code, userId, cancellationToken);
        if (room is null)
        {
            return null;
        }

        room.Code = NewCode();
        await db.SaveChangesAsync(cancellationToken);
        return room.Code;
    }

    /// <summary>O dono remove outro membro. Para sair, o próprio dono usa <see cref="LeaveAsync"/>.</summary>
    public async Task<RoomActionOutcome> RemoveMemberAsync(string code, string userId, int memberId, CancellationToken cancellationToken = default)
    {
        var (room, outcome) = await OwnedAsync(code, userId, cancellationToken);
        if (room is null)
        {
            return outcome;
        }

        if (room.Members.FirstOrDefault(m => m.Id == memberId) is not { } member || member.UserId == userId)
        {
            return RoomActionOutcome.NotFound;
        }

        room.Members.Remove(member);
        await db.SaveChangesAsync(cancellationToken);
        return RoomActionOutcome.Done;
    }

    public async Task<RoomActionOutcome> DeleteAsync(string code, string userId, CancellationToken cancellationToken = default)
    {
        var (room, outcome) = await OwnedAsync(code, userId, cancellationToken);
        if (room is null)
        {
            return outcome;
        }

        db.StudyRooms.Remove(room);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Sala {RoomId} apagada pelo dono.", room.Id);
        return RoomActionOutcome.Done;
    }

    /// <summary>
    /// Os membros como aparecem para os colegas: primeiro nome e inicial do sobrenome, o dono primeiro e depois por
    /// ordem de entrada. A página só mostra a lista a quem é membro.
    /// </summary>
    public async Task<IReadOnlyList<RoomMemberView>> MembersAsync(StudyRoom room, string? userId, CancellationToken cancellationToken = default)
    {
        var ids = room.Members.Select(m => m.UserId).ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        return room.Members
            .OrderByDescending(m => m.UserId == room.OwnerId).ThenBy(m => m.Id)
            .Select(m => new RoomMemberView(m.Id, DuelService.ShortName(names.GetValueOrDefault(m.UserId)), m.UserId == room.OwnerId, m.UserId == userId))
            .ToList();
    }

    /// <summary>As salas da pessoa em todas as trilhas, da mais nova para a mais antiga.</summary>
    public async Task<IReadOnlyList<RoomSummary>> MineAsync(string userId, CancellationToken cancellationToken = default) =>
        await db.StudyRooms.AsNoTracking()
            .Where(r => r.Members.Any(m => m.UserId == userId))
            .OrderByDescending(r => r.Id)
            .Select(r => new RoomSummary(r.Code, r.Name, r.TrackId, r.Members.Count, r.OwnerId == userId))
            .ToListAsync(cancellationToken);

    /// <summary>Um membro denuncia a sala. Cada pessoa tem no máximo uma denúncia aberta por sala.</summary>
    public async Task<RoomActionOutcome> ReportAsync(string code, string userId, string? reason, CancellationToken cancellationToken = default)
    {
        var room = await FindAsync(code, cancellationToken);
        if (room is null)
        {
            return RoomActionOutcome.NotFound;
        }

        if (room.Members.All(m => m.UserId != userId))
        {
            return RoomActionOutcome.NotAllowed;
        }

        if (!await db.StudyRoomReports.AnyAsync(r => r.RoomId == room.Id && r.ReporterId == userId && r.ResolvedAt == null, cancellationToken))
        {
            var trimmed = (reason ?? "").Trim();
            db.StudyRoomReports.Add(new StudyRoomReport
            {
                RoomId = room.Id,
                ReporterId = userId,
                Reason = trimmed.Length == 0 ? null : trimmed[..Math.Min(trimmed.Length, ReasonMaxLength)],
                CreatedAt = clock.GetUtcNow(),
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        return RoomActionOutcome.Done;
    }

    /// <summary>Salas com denúncia aberta, da mais antiga para a mais nova. A moderação vê o nome da sala, não os membros.</summary>
    public async Task<IReadOnlyList<RoomReportGroup>> OpenReportsAsync(CancellationToken cancellationToken = default)
    {
        var reports = await db.StudyRoomReports.AsNoTracking()
            .Where(r => r.ResolvedAt == null)
            .Join(db.StudyRooms, r => r.RoomId, room => room.Id,
                (r, room) => new { r.Id, r.RoomId, r.Reason, r.CreatedAt, room.Name, room.TrackId, Members = room.Members.Count })
            .ToListAsync(cancellationToken);

        return reports.GroupBy(r => r.RoomId)
            .Select(group =>
            {
                var first = group.OrderBy(r => r.Id).First();
                return new RoomReportGroup(group.Key, first.Name, first.TrackId, first.Members, group.Count(), first.CreatedAt,
                    group.OrderBy(r => r.Id).Select(r => r.Reason).OfType<string>().ToList());
            })
            .OrderBy(group => group.FirstReportedAt)
            .ToList();
    }

    /// <summary>Encerra as denúncias abertas de uma sala. Falso se não havia nenhuma (outra pessoa já resolveu).</summary>
    public async Task<bool> ModerateAsync(int roomId, string moderatorId, RoomModeration decision, CancellationToken cancellationToken = default)
    {
        var open = await db.StudyRoomReports.Where(r => r.RoomId == roomId && r.ResolvedAt == null).ToListAsync(cancellationToken);
        var room = await db.StudyRooms.FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
        if (open.Count == 0 || room is null)
        {
            return false;
        }

        if (decision == RoomModeration.Delete)
        {
            // As denúncias saem junto com a sala.
            db.StudyRooms.Remove(room);
        }
        else
        {
            if (decision == RoomModeration.ResetName)
            {
                room.Name = NeutralName;
            }

            var now = clock.GetUtcNow();
            foreach (var report in open)
            {
                report.ResolvedAt = now;
                report.ResolvedById = moderatorId;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Sala {RoomId} moderada: {Decision}.", roomId, decision);
        return true;
    }

    /// <summary>Nome sem espaços sobrando e sem caracteres de controle; nulo se ficar fora do tamanho aceito.</summary>
    public static string? NormalizeName(string? name)
    {
        var cleaned = Whitespace().Replace(new string((name ?? "").Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray()), " ").Trim();
        return cleaned.Length is >= NameMinLength and <= NameMaxLength ? cleaned : null;
    }

    public static string Normalize(string code) => code.Trim().ToLowerInvariant();

    private async Task<(StudyRoom? Room, RoomActionOutcome Outcome)> OwnedAsync(string code, string userId, CancellationToken cancellationToken)
    {
        var room = await FindAsync(code, cancellationToken);
        return room is null ? (null, RoomActionOutcome.NotFound)
            : room.OwnerId != userId ? (null, RoomActionOutcome.NotAllowed)
            : (room, RoomActionOutcome.Done);
    }

    private void Remove(StudyRoom room, StudyRoomMember member)
    {
        room.Members.Remove(member);
        if (room.Members.Count == 0)
        {
            db.StudyRooms.Remove(room);
        }
        else if (room.OwnerId == member.UserId)
        {
            room.OwnerId = Oldest(room).UserId;
        }
    }

    private static StudyRoomMember Oldest(StudyRoom room) => room.Members.MinBy(m => m.Id)!;

    private static string NewCode() => RandomNumberGenerator.GetString(CodeAlphabet, CodeLength);

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
