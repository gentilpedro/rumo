using EnadeAds.Achievements;
using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Friends;

/// <summary>
/// O que um amigo vê do outro, no espírito da "atividade dos amigos" do Spotify: a prova, o nível, a sequência de
/// dias e o que a pessoa estudou hoje. <see cref="Shares"/> falso é quem desligou a chave: só o nome aparece.
/// Nunca entram aqui quais questões a pessoa respondeu, o que acertou ou errou, notas de simulado nem horários.
/// </summary>
public sealed record FriendActivity(
    int FriendshipId,
    string Name,
    DateTimeOffset Since,
    bool Shares,
    int? TrackId,
    Level? Level,
    int Streak,
    int AnsweredToday,
    string? Subject,
    DateOnly? LastStudied);

public sealed record FriendActivityPage(IReadOnlyList<FriendActivity> Items, int Page, int TotalPages, int TotalCount);

/// <summary>
/// Atividade de estudo dos amigos aceitos. Nada é gravado além da chave <see cref="ApplicationUser.ShareActivity"/>:
/// tudo sai das respostas que já existem, como o XP.
/// </summary>
public sealed class FriendActivityService(ApplicationDbContext db, AchievementService achievements, LocalCalendar calendar)
{
    /// <summary>Amigos por página. O nível e a sequência pedem várias consultas por pessoa, então a lista vem aos poucos.</summary>
    public const int PageSize = 20;

    public Task<bool> SharesAsync(string userId, CancellationToken cancellationToken = default) =>
        db.Users.Where(u => u.Id == userId).Select(u => u.ShareActivity).FirstOrDefaultAsync(cancellationToken);

    /// <summary>Liga ou desliga a chave "mostrar minha atividade aos amigos". As amizades continuam as mesmas.</summary>
    public async Task SetSharesAsync(string userId, bool shares, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        user.ShareActivity = shares;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Os amigos da pessoa, de quem estudou mais recentemente para quem estudou há mais tempo.</summary>
    public async Task<FriendActivityPage> ListAsync(string userId, int page, CancellationToken cancellationToken = default)
    {
        // Só amizade aceita: pedido pendente não vê nada.
        var friendships = await db.Friendships.AsNoTracking()
            .Where(f => f.AcceptedAt != null && (f.RequesterId == userId || f.AddresseeId == userId))
            .ToListAsync(cancellationToken);
        var ids = friendships.Select(f => f.OtherThan(userId)).ToList();
        var people = await db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.ShareActivity })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var sharing = people.Values.Where(p => p.ShareActivity).Select(p => p.Id).ToList();
        var lastAttempt = await db.QuestionAttempts.AsNoTracking()
            .Where(a => sharing.Contains(a.UserId))
            .GroupBy(a => a.UserId)
            .Select(g => new { UserId = g.Key, Last = g.Max(a => a.Id) })
            .ToDictionaryAsync(g => g.UserId, g => g.Last, cancellationToken);

        var ordered = friendships
            .Where(f => people.ContainsKey(f.OtherThan(userId)))
            .Select(f => (Friendship: f, Person: people[f.OtherThan(userId)]))
            .OrderByDescending(x => x.Person.ShareActivity ? lastAttempt.GetValueOrDefault(x.Person.Id) : -1)
            .ThenBy(x => x.Person.FullName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var totalPages = Math.Max(1, (int)Math.Ceiling(ordered.Count / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var items = new List<FriendActivity>();
        foreach (var (friendship, person) in ordered.Skip((page - 1) * PageSize).Take(PageSize))
        {
            var name = DuelService.ShortName(person.FullName);
            var since = friendship.AcceptedAt!.Value;
            if (!person.ShareActivity)
            {
                items.Add(new FriendActivity(friendship.Id, name, since, Shares: false, null, null, 0, 0, null, null));
                continue;
            }

            var report = await achievements.GetAsync(person.Id, cancellationToken);
            var last = await db.QuestionAttempts.AsNoTracking()
                .Where(a => a.UserId == person.Id)
                .OrderByDescending(a => a.Id)
                .Take(1)
                .Join(db.Questions.IgnoreQueryFilters(), a => a.QuestionId, q => q.Id, (a, q) => new { a.AnsweredAt, q.Subject!.Title, q.Subject.TrackId })
                .FirstOrDefaultAsync(cancellationToken);

            // Só o dia da última resposta, nunca a hora.
            DateOnly? lastDay = last is null ? null : DateOnly.FromDateTime(calendar.ToLocal(last.AnsweredAt));
            items.Add(new FriendActivity(friendship.Id, name, since, Shares: true, last?.TrackId, report.Level, report.CurrentStreak,
                report.AnsweredToday, lastDay == calendar.Today() ? last!.Title : null, lastDay));
        }

        return new FriendActivityPage(items, page, totalPages, ordered.Count);
    }
}
