using EnadeAds.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Accounts;

public sealed record AccountRow(string Id, string FullName, string Email, bool EmailConfirmed, IReadOnlyList<string> Roles, bool IsBlocked);

public sealed record AccountPage(IReadOnlyList<AccountRow> Items, int Page, int TotalPages, int TotalCount);

public enum AccountFilter
{
    All,
    Students,
    PendingTeachers,
    Teachers,
    Admins,
    Blocked,
}

public enum AccountAction
{
    MakeTeacher,
    RevokeTeacher,
    MakeAdmin,
    RemoveAdmin,
    Block,
    Unblock,
}

/// <summary>
/// Gestão de contas pelo admin. Toda mudança renova o security stamp, então vale em até 1 minuto
/// (revalidação da sessão definida na #3). Um admin nunca altera a própria conta por aqui.
///
/// Dar um perfil (professor ou admin) só vale para conta com e-mail confirmado e não bloqueada: sem a
/// confirmação, não dá para saber se a conta é mesmo de quem diz ser.
/// </summary>
public sealed class AccountAdmin(ApplicationDbContext db, UserManager<ApplicationUser> users, TimeProvider clock, ILogger<AccountAdmin> logger)
{
    public const int PageSize = 25;

    public async Task<AccountPage> SearchAsync(string? text, AccountFilter filter, int page, CancellationToken cancellationToken = default)
    {
        var query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var term = text.Trim().ToUpperInvariant();
            query = query.Where(u => u.NormalizedEmail!.Contains(term) || u.FullName.ToUpper().Contains(term));
        }

        var now = clock.GetUtcNow();
        var roleIds = await db.Roles.AsNoTracking().ToDictionaryAsync(r => r.Name!, r => r.Id, cancellationToken);
        var teacher = roleIds.GetValueOrDefault(Roles.Teacher);
        var pending = roleIds.GetValueOrDefault(Roles.PendingTeacher);
        var admin = roleIds.GetValueOrDefault(Roles.Admin);
        var userRoles = db.UserRoles;

        query = filter switch
        {
            AccountFilter.Students => query.Where(u => !userRoles.Any(ur => ur.UserId == u.Id && (ur.RoleId == teacher || ur.RoleId == pending || ur.RoleId == admin))),
            AccountFilter.PendingTeachers => query.Where(u => userRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == pending)),
            AccountFilter.Teachers => query.Where(u => userRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == teacher)),
            AccountFilter.Admins => query.Where(u => userRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == admin)),
            AccountFilter.Blocked => query.Where(u => u.LockoutEnd != null && u.LockoutEnd > now),
            _ => query,
        };

        var total = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var accounts = await query
            .OrderBy(u => u.FullName).ThenBy(u => u.Email)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(u => new { u.Id, u.FullName, u.Email, u.EmailConfirmed, u.LockoutEnd })
            .ToListAsync(cancellationToken);

        var ids = accounts.Select(a => a.Id).ToList();
        var roles = await db.UserRoles.AsNoTracking()
            .Where(ur => ids.Contains(ur.UserId))
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync(cancellationToken);

        var items = accounts.Select(a => new AccountRow(
            a.Id, a.FullName, a.Email ?? "", a.EmailConfirmed,
            roles.Where(r => r.UserId == a.Id).Select(r => r.Name!).OrderBy(n => n).ToList(),
            a.LockoutEnd is { } end && end > now)).ToList();

        return new AccountPage(items, page, totalPages, total);
    }

    public async Task<EditResultMessage> ApplyAsync(string adminId, string targetId, AccountAction action, CancellationToken cancellationToken = default)
    {
        // Nada de mexer na própria conta: evita o admin se trancar fora ou ficar sem admin por engano.
        if (adminId == targetId)
        {
            return EditResultMessage.Fail("Você não pode alterar a própria conta por aqui.");
        }

        var user = await users.FindByIdAsync(targetId);
        if (user is null)
        {
            return EditResultMessage.Fail("Conta não encontrada.");
        }

        var name = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
        string message;
        switch (action)
        {
            case AccountAction.MakeTeacher:
                if (await users.IsInRoleAsync(user, Roles.Teacher))
                {
                    return EditResultMessage.Fail($"{name} já é professor.");
                }

                if (await CannotBePromotedAsync(user, name) is { } teacherProblem)
                {
                    return teacherProblem;
                }

                // Decisão do admin: vale sem e-mail institucional e sem pedido. Se havia pedido pendente, ele se encerra aqui.
                if (await users.IsInRoleAsync(user, Roles.PendingTeacher))
                {
                    Ensure(await users.RemoveFromRoleAsync(user, Roles.PendingTeacher));
                }

                Ensure(await users.AddToRoleAsync(user, Roles.Teacher));
                message = $"{name} agora é professor.";
                break;

            case AccountAction.MakeAdmin:
                if (await users.IsInRoleAsync(user, Roles.Admin))
                {
                    return EditResultMessage.Fail($"{name} já é administrador.");
                }

                if (await CannotBePromotedAsync(user, name) is { } adminProblem)
                {
                    return adminProblem;
                }

                Ensure(await users.AddToRoleAsync(user, Roles.Admin));
                message = $"{name} agora é administrador.";
                break;

            case AccountAction.RevokeTeacher:
                if (!await users.IsInRoleAsync(user, Roles.Teacher))
                {
                    return EditResultMessage.Fail($"{name} não é professor.");
                }

                Ensure(await users.RemoveFromRoleAsync(user, Roles.Teacher));
                message = $"{name} deixou de ser professor e continua como aluno.";
                break;

            case AccountAction.RemoveAdmin:
                if (!await users.IsInRoleAsync(user, Roles.Admin))
                {
                    return EditResultMessage.Fail($"{name} não é administrador.");
                }

                Ensure(await users.RemoveFromRoleAsync(user, Roles.Admin));
                message = $"{name} deixou de ser administrador.";
                break;

            case AccountAction.Block:
                Ensure(await users.SetLockoutEnabledAsync(user, true));
                Ensure(await users.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue));
                message = $"{name} foi bloqueado e não consegue mais entrar.";
                break;

            case AccountAction.Unblock:
                Ensure(await users.SetLockoutEndDateAsync(user, null));
                Ensure(await users.ResetAccessFailedCountAsync(user));
                message = $"{name} foi desbloqueado.";
                break;

            default:
                return EditResultMessage.Fail("Ação inválida.");
        }

        // Derruba a sessão atual da pessoa: o novo perfil (ou o bloqueio) vale na próxima revalidação.
        Ensure(await users.UpdateSecurityStampAsync(user));
        logger.LogInformation("Admin {AdminId} aplicou {Action} na conta {UserId}.", adminId, action, targetId);
        return new EditResultMessage(true, message);
    }

    /// <summary>Motivo para não dar um perfil a esta conta, ou nulo se pode.</summary>
    private async Task<EditResultMessage?> CannotBePromotedAsync(ApplicationUser user, string? name)
    {
        if (!user.EmailConfirmed)
        {
            return EditResultMessage.Fail($"{name} ainda não confirmou o e-mail. Só contas com e-mail confirmado recebem um perfil.");
        }

        if (await users.IsLockedOutAsync(user))
        {
            return EditResultMessage.Fail($"{name} está com a conta bloqueada. Desbloqueie antes de dar um perfil.");
        }

        return null;
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}

public sealed record EditResultMessage(bool Succeeded, string Message)
{
    public static EditResultMessage Fail(string message) => new(false, message);
}
