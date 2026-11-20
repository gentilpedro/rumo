using EnadeAds.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Privacy;

/// <summary>
/// Monta o arquivo "Baixar meus dados" (direito de acesso da LGPD): conta e tudo o que a pessoa fez na plataforma.
/// </summary>
public sealed class PersonalDataExport(ApplicationDbContext db, UserManager<ApplicationUser> users)
{
    public async Task<object> BuildAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var userId = user.Id;

        // A exportação cobre todas as trilhas, não só a ativa: por isso IgnoreQueryFilters.

        var roadmap = await db.RoadmapProgress.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Join(db.RoadmapNodes.IgnoreQueryFilters(), p => p.NodeId, n => n.Id, (p, n) => new { Topico = n.Title, Status = p.Status.ToString(), Atualizado = p.UpdatedAt })
            .ToListAsync(cancellationToken);

        var attempts = await db.QuestionAttempts.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Join(db.QuestionOptions, a => a.OptionId, o => o.Id,
                (a, o) => new { Questao = a.QuestionId, Alternativa = o.Letter.ToString(), Acertou = a.IsCorrect, Desafio = a.ChallengeId, Em = a.AnsweredAt })
            .ToListAsync(cancellationToken);

        var discursives = await db.DiscursiveAnswers.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new { Questao = a.QuestionId, Resposta = a.Text, Em = a.AnsweredAt })
            .ToListAsync(cancellationToken);

        var challenges = await db.ChallengeSubmissions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new { Desafio = s.ChallengeId, Acertos = s.CorrectCount, Questoes = s.QuestionCount, Em = s.SubmittedAt })
            .ToListAsync(cancellationToken);

        var mockExams = await db.MockExams.IgnoreQueryFilters().AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => new { Trilha = m.TrackId, Tamanho = m.Size.ToString(), Inicio = m.StartedAt, Entrega = m.SubmittedAt, Acertos = m.CorrectCount, Objetivas = m.ObjectiveCount, IgnoroAResposta = m.IgnoredCount })
            .ToListAsync(cancellationToken);

        var duels = await db.DuelPlayers.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Join(db.Duels.IgnoreQueryFilters(), p => p.DuelId, d => d.Id,
                (p, d) => new { Duelo = d.Code, Trilha = d.TrackId, CriadoPorVoce = d.CreatorId == userId, Inicio = p.StartedAt, Envio = p.FinishedAt, Acertos = p.CorrectCount })
            .ToListAsync(cancellationToken);

        var rooms = await db.StudyRoomMembers.AsNoTracking()
            .Where(m => m.UserId == userId)
            .Join(db.StudyRooms, m => m.RoomId, r => r.Id,
                (m, r) => new { Sala = r.Name, Trilha = r.TrackId, CriadaPorVoce = r.OwnerId == userId, EntrouEm = m.JoinedAt })
            .ToListAsync(cancellationToken);

        var roomReports = await db.StudyRoomReports.AsNoTracking()
            .Where(r => r.ReporterId == userId)
            .Join(db.StudyRooms, r => r.RoomId, room => room.Id, (r, room) => new { Sala = room.Name, Motivo = r.Reason, Em = r.CreatedAt })
            .ToListAsync(cancellationToken);
        // Dos amigos vai só o nome abreviado, como a pessoa os vê na plataforma: o arquivo é dela, não deles.
        var relations = await db.Friendships.AsNoTracking()
            .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
            .ToListAsync(cancellationToken);
        var blockedRows = await db.FriendBlocks.AsNoTracking().Where(b => b.BlockerId == userId).ToListAsync(cancellationToken);
        var otherIds = relations.Select(f => f.OtherThan(userId)).Concat(blockedRows.Select(b => b.BlockedId)).Distinct().ToList();
        var otherNames = await db.Users.AsNoTracking()
            .Where(u => otherIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, u => Duels.DuelService.ShortName(u.FullName), cancellationToken);
        var friends = relations.Select(f => new
        {
            Pessoa = otherNames.GetValueOrDefault(f.OtherThan(userId)), PedidoPorVoce = f.RequesterId == userId, PedidoEm = f.CreatedAt, AceitoEm = f.AcceptedAt,
        }).ToList();
        var blocked = blockedRows.Select(b => new { Pessoa = otherNames.GetValueOrDefault(b.BlockedId), Em = b.CreatedAt }).ToList();

        var friendLink = await db.FriendLinks.AsNoTracking().Where(l => l.UserId == userId).Select(l => l.Code).FirstOrDefaultAsync(cancellationToken);

        var comments = await db.PostComments.AsNoTracking()
            .Where(c => c.UserId == userId)
            .Join(db.Posts.IgnoreQueryFilters(), c => c.PostId, p => p.Id, (c, p) => new { Post = p.Title, Comentario = c.Text, Em = c.CreatedAt })
            .ToListAsync(cancellationToken);

        var posts = await db.Posts.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.AuthorId == userId)
            .Select(p => new { Titulo = p.Title, Texto = p.Body, Criado = p.CreatedAt, Publicado = p.PublishedAt, Removido = p.RemovedAt })
            .ToListAsync(cancellationToken);

        var votes = await db.PostVotes.AsNoTracking()
            .Where(v => v.UserId == userId)
            .Join(db.Posts.IgnoreQueryFilters(), v => v.PostId, p => p.Id, (v, p) => new { Post = p.Title, Voto = v.Value, Em = v.CreatedAt })
            .ToListAsync(cancellationToken);

        var reports = await db.PostReports.AsNoTracking()
            .Where(r => r.ReporterId == userId)
            .Join(db.Posts.IgnoreQueryFilters(), r => r.PostId, p => p.Id, (r, p) => new { Post = p.Title, Comentario = r.CommentId, Motivo = r.Reason, Em = r.CreatedAt })
            .ToListAsync(cancellationToken);

        return new
        {
            Conta = new
            {
                Nome = user.FullName,
                user.Email,
                EmailConfirmado = user.EmailConfirmed,
                Telefone = user.PhoneNumber,
                Perfis = await users.GetRolesAsync(user),
                TermosAceitosEm = user.TermsAcceptedAt,
                VersaoDosTermos = user.TermsVersion,
                MenorDeIdade = user.IsMinor,
                MetaDiaria = user.DailyGoal,
                AutorizacaoDoResponsavelEm = user.GuardianConsentAt,
                LoginsExternos = (await users.GetLoginsAsync(user)).Select(l => l.LoginProvider),
            },
            Roadmap = roadmap,
            RespostasObjetivas = attempts,
            RespostasDiscursivas = discursives,
            Desafios = challenges,
            Simulados = mockExams,
            Duelos = duels,
            SalasDeEstudo = rooms,
            DenunciasDeSalas = roomReports,
            LinkDeAmigo = friendLink,
            AmigosEPedidos = friends,
            Bloqueados = blocked,
            ComentariosNoBlog = comments,
            Posts = posts,
            Votos = votes,
            Denuncias = reports,
        };
    }
}
