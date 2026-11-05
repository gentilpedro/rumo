using EnadeAds.Blog;
using EnadeAds.Challenges;
using EnadeAds.Daily;
using EnadeAds.Duels;
using EnadeAds.MockExams;
using EnadeAds.Questions;
using EnadeAds.Reviews;
using EnadeAds.Study;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EnadeAds.Data;

/// <param name="trackScope">Trilha ativa da requisição. Sem ela (ferramentas de migration) vale a trilha padrão.</param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, TrackScope? trackScope = null) : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>
    /// Trilha usada pelos filtros globais: roadmap, questões, desafios, simulados e posts só aparecem na trilha
    /// deles. Para ver tudo (exportação de dados da conta, importação), use <c>IgnoreQueryFilters()</c>.
    /// </summary>
    public int CurrentTrackId => trackScope?.Id ?? EnadeAds.Study.Tracks.Default;

    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<RoadmapNode> RoadmapNodes => Set<RoadmapNode>();
    public DbSet<RoadmapProgress> RoadmapProgress => Set<RoadmapProgress>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuestionAttempt> QuestionAttempts => Set<QuestionAttempt>();
    public DbSet<DiscursiveAnswer> DiscursiveAnswers => Set<DiscursiveAnswer>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeQuestion> ChallengeQuestions => Set<ChallengeQuestion>();
    public DbSet<ChallengeSubmission> ChallengeSubmissions => Set<ChallengeSubmission>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<PostVote> PostVotes => Set<PostVote>();
    public DbSet<PostReport> PostReports => Set<PostReport>();
    public DbSet<MockExam> MockExams => Set<MockExam>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<MockExamQuestion> MockExamQuestions => Set<MockExamQuestion>();
    public DbSet<ContentReview> ContentReviews => Set<ContentReview>();
    public DbSet<Duel> Duels => Set<Duel>();
    public DbSet<DuelQuestion> DuelQuestions => Set<DuelQuestion>();
    public DbSet<DuelPlayer> DuelPlayers => Set<DuelPlayer>();
    public DbSet<DuelAnswer> DuelAnswers => Set<DuelAnswer>();
    public DbSet<DailyQuestion> DailyQuestions => Set<DailyQuestion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Track>(track =>
        {
            track.Property(t => t.Id).ValueGeneratedNever();
            track.Property(t => t.Slug).HasMaxLength(40);
            track.Property(t => t.Name).HasMaxLength(80);
            track.Property(t => t.ExamName).HasMaxLength(40);
            track.Property(t => t.Label).HasMaxLength(40);
            track.Property(t => t.Basis).HasMaxLength(300);
            track.HasIndex(t => t.Slug).IsUnique();
            track.Ignore(t => t.ExamDays);
            track.Ignore(t => t.LastExamDay);
            track.HasData(EnadeAds.Study.Tracks.Seed.Select(t => new Track
            {
                Id = t.Id, Slug = t.Slug, Name = t.Name, ExamName = t.ExamName, Label = t.Label, Basis = t.Basis, Level = t.Level,
                MockExamStyle = t.MockExamStyle, ExamDate = t.ExamDate, SecondExamDate = t.SecondExamDate, Order = t.Order,
            }));
        });

        builder.Entity<RoadmapNode>(node =>
        {
            node.Property(n => n.Id).ValueGeneratedNever();
            node.Property(n => n.Slug).HasMaxLength(80);
            node.Property(n => n.Title).HasMaxLength(200);
            node.Property(n => n.Summary).HasMaxLength(2000);
            // O mesmo slug pode existir em trilhas diferentes ("matematica" no ENEM e num seletivo de IF).
            node.HasIndex(n => new { n.TrackId, n.Slug }).IsUnique();
            node.HasOne<Track>().WithMany().HasForeignKey(n => n.TrackId).OnDelete(DeleteBehavior.Restrict);
            node.HasQueryFilter(n => n.TrackId == CurrentTrackId);
            node.HasOne(n => n.Parent)
                .WithMany(n => n.Topics)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
            node.HasData(RoadmapSeed.Nodes.Concat(EnemRoadmapSeed.Nodes).Concat(RedesRoadmapSeed.Nodes).Select(n => new RoadmapNode
            {
                Id = n.Id, TrackId = n.TrackId, Slug = n.Slug, Title = n.Title, Summary = n.Summary,
                Kind = n.Kind, Block = n.Block, Order = n.Order, ParentId = n.ParentId,
            }));
        });

        builder.Entity<RoadmapProgress>(progress =>
        {
            progress.HasKey(p => new { p.UserId, p.NodeId });
            progress.HasOne<ApplicationUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            progress.HasOne<RoadmapNode>().WithMany().HasForeignKey(p => p.NodeId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Question>(question =>
        {
            question.Property(q => q.Statement).HasMaxLength(8000);
            question.Property(q => q.Source).HasMaxLength(200);
            question.Property(q => q.ExternalKey).HasMaxLength(60);
            question.HasIndex(q => q.ExternalKey).IsUnique().HasFilter("\"ExternalKey\" IS NOT NULL");
            question.Property(q => q.ExpectedAnswer).HasMaxLength(QuestionEditor.ExpectedAnswerMaxLength);
            question.HasOne(q => q.Subject).WithMany().HasForeignKey(q => q.SubjectId).OnDelete(DeleteBehavior.Restrict);
            question.HasOne<ApplicationUser>().WithMany().HasForeignKey(q => q.AuthorId).OnDelete(DeleteBehavior.SetNull);
            question.HasMany(q => q.Options).WithOne().HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
            question.HasQueryFilter(q => q.Subject!.TrackId == CurrentTrackId);
            question.HasData(QuestionSeed.Questions.Select(q => new Question
            {
                Id = q.Id, Kind = q.Kind, SubjectId = q.SubjectId, Statement = q.Statement, Source = q.Source, CreatedAt = q.CreatedAt,
                ExpectedAnswer = q.ExpectedAnswer, SuggestedLines = q.SuggestedLines,
            }));
        });

        builder.Entity<QuestionOption>(option =>
        {
            option.Property(o => o.Text).HasMaxLength(2000);
            option.Property(o => o.Explanation).HasMaxLength(2000);
            option.HasIndex(o => new { o.QuestionId, o.Letter }).IsUnique();
            option.HasData(QuestionSeed.Options);
        });

        builder.Entity<QuestionAttempt>(attempt =>
        {
            attempt.HasIndex(a => new { a.UserId, a.QuestionId });
            attempt.HasOne<ApplicationUser>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            attempt.HasOne<Question>().WithMany().HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.Cascade);
            attempt.HasOne<QuestionOption>().WithMany().HasForeignKey(a => a.OptionId).OnDelete(DeleteBehavior.Cascade);
            attempt.HasOne<Challenge>().WithMany().HasForeignKey(a => a.ChallengeId).OnDelete(DeleteBehavior.Cascade);
            attempt.HasIndex(a => a.ChallengeId);
        });

        builder.Entity<Challenge>(challenge =>
        {
            challenge.Property(c => c.Title).HasMaxLength(ChallengeService.TitleMaxLength);
            challenge.Property(c => c.Description).HasMaxLength(ChallengeService.DescriptionMaxLength);
            challenge.HasOne<ApplicationUser>().WithMany().HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.SetNull);
            challenge.HasMany(c => c.Questions).WithOne().HasForeignKey(q => q.ChallengeId).OnDelete(DeleteBehavior.Cascade);
            challenge.HasOne<Track>().WithMany().HasForeignKey(c => c.TrackId).OnDelete(DeleteBehavior.Restrict);
            challenge.HasQueryFilter(c => c.TrackId == CurrentTrackId);
        });

        builder.Entity<ChallengeQuestion>(question =>
        {
            question.HasKey(q => new { q.ChallengeId, q.QuestionId });
            question.HasOne(q => q.Question).WithMany().HasForeignKey(q => q.QuestionId).OnDelete(DeleteBehavior.Restrict);
            // Mesmo filtro da questão, para o EF não carregar um item cuja questão ficou de fora.
            question.HasQueryFilter(q => q.Question!.Subject!.TrackId == CurrentTrackId);
        });

        builder.Entity<ChallengeSubmission>(submission =>
        {
            // Um envio por aluno em cada desafio.
            submission.HasKey(s => new { s.ChallengeId, s.UserId });
            submission.HasOne<Challenge>().WithMany().HasForeignKey(s => s.ChallengeId).OnDelete(DeleteBehavior.Cascade);
            submission.HasOne<ApplicationUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Post>(post =>
        {
            post.Property(p => p.Slug).HasMaxLength(Slugs.MaxLength + 10);
            post.Property(p => p.Title).HasMaxLength(BlogService.TitleMaxLength);
            post.Property(p => p.Summary).HasMaxLength(BlogService.SummaryMaxLength);
            post.Property(p => p.Body).HasMaxLength(BlogService.BodyMaxLength);
            post.Property(p => p.SourceUrl).HasMaxLength(BlogService.SourceMaxLength);
            post.HasIndex(p => p.Slug).IsUnique();
            post.HasIndex(p => p.PublishedAt);
            post.Ignore(p => p.IsPublished);
            post.HasOne<ApplicationUser>().WithMany().HasForeignKey(p => p.RemovedById).OnDelete(DeleteBehavior.SetNull);
            post.HasOne<ApplicationUser>().WithMany().HasForeignKey(p => p.AuthorId).OnDelete(DeleteBehavior.SetNull);
            post.HasOne<Track>().WithMany().HasForeignKey(p => p.TrackId).OnDelete(DeleteBehavior.Restrict);
            // O feed mostra os posts da trilha ativa e os da plataforma inteira.
            post.HasQueryFilter(p => p.TrackId == null || p.TrackId == CurrentTrackId);
        });

        builder.Entity<PostComment>(comment =>
        {
            comment.Property(c => c.Text).HasMaxLength(BlogService.CommentMaxLength);
            comment.HasIndex(c => c.PostId);
            comment.HasOne<Post>().WithMany().HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
            // Excluir a conta mantém o comentário, sem autor: a conversa do post não perde o sentido.
            comment.HasOne<ApplicationUser>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<PostVote>(vote =>
        {
            vote.HasKey(v => new { v.PostId, v.UserId });
            vote.HasIndex(v => v.UserId);
            vote.HasOne<Post>().WithMany().HasForeignKey(v => v.PostId).OnDelete(DeleteBehavior.Cascade);
            vote.HasOne<ApplicationUser>().WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostReport>(report =>
        {
            report.Property(r => r.Reason).HasMaxLength(BlogService.ReportReasonMaxLength);
            report.HasIndex(r => r.ResolvedAt);
            report.HasOne<Post>().WithMany().HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
            report.HasOne<PostComment>().WithMany().HasForeignKey(r => r.CommentId).OnDelete(DeleteBehavior.Cascade);
            report.HasOne<ApplicationUser>().WithMany().HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.SetNull);
            report.HasOne<ApplicationUser>().WithMany().HasForeignKey(r => r.ResolvedById).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ApplicationUser>(user => user.Property(u => u.TermsVersion).HasMaxLength(20));

        builder.Entity<SiteSetting>(setting =>
        {
            setting.HasKey(s => s.Key);
            setting.Property(s => s.Key).HasMaxLength(100);
            setting.Property(s => s.Value).HasMaxLength(1000);
        });

        builder.Entity<MockExam>(exam =>
        {
            exam.Ignore(m => m.IsOpen);
            exam.HasOne<ApplicationUser>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            exam.HasMany(m => m.Questions).WithOne().HasForeignKey(q => q.MockExamId).OnDelete(DeleteBehavior.Cascade);

            exam.HasOne<Track>().WithMany().HasForeignKey(m => m.TrackId).OnDelete(DeleteBehavior.Restrict);
            exam.HasQueryFilter(m => m.TrackId == CurrentTrackId);

            // Um simulado em andamento por aluno em cada trilha, garantido pelo banco (o filtro funciona no PostgreSQL e no SQLite).
            exam.HasIndex(m => new { m.UserId, m.TrackId }).IsUnique().HasFilter("\"SubmittedAt\" IS NULL").HasDatabaseName("IX_MockExams_OpenPerUser");
            exam.HasIndex(m => new { m.UserId, m.Id });
        });

        builder.Entity<MockExamQuestion>(question =>
        {
            question.HasKey(q => new { q.MockExamId, q.QuestionId });
            question.Property(q => q.DiscursiveText).HasMaxLength(MockExamService.DiscursiveMaxLength);
            question.HasOne(q => q.Question).WithMany().HasForeignKey(q => q.QuestionId).OnDelete(DeleteBehavior.Restrict);
            question.HasQueryFilter(q => q.Question!.Subject!.TrackId == CurrentTrackId);
            question.HasOne<Questions.QuestionOption>().WithMany().HasForeignKey(q => q.SelectedOptionId).OnDelete(DeleteBehavior.SetNull);
        });

        // O SQLite (usado nos testes) não ordena nem compara DateTimeOffset; guarda como número só nele.
        // Receita da documentação do EF Core; no PostgreSQL nada muda.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(DateTimeOffset) || p.ClrType == typeof(DateTimeOffset?)))
            {
                property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
            }
        }

        builder.Entity<ContentReview>(review =>
        {
            // Sem filtro de trilha: quem revisa vê as propostas de todas as trilhas, cada uma com a sua.
            review.Property(r => r.Summary).HasMaxLength(ContentReview.SummaryMaxLength);
            review.Property(r => r.Note).HasMaxLength(ContentReview.NoteMaxLength);
            review.HasIndex(r => new { r.Status, r.CreatedAt });
            review.HasIndex(r => r.AuthorId);
            review.HasOne<Track>().WithMany().HasForeignKey(r => r.TrackId).OnDelete(DeleteBehavior.Restrict);
            // Excluir a conta apaga as propostas dela; quem revisou só perde o vínculo.
            review.HasOne<ApplicationUser>().WithMany().HasForeignKey(r => r.AuthorId).OnDelete(DeleteBehavior.Cascade);
            review.HasOne<ApplicationUser>().WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Duel>(duel =>
        {
            duel.Property(d => d.Code).HasMaxLength(DuelService.CodeLength);
            duel.HasIndex(d => d.Code).IsUnique();
            duel.Ignore(d => d.JoinUntil);
            duel.HasOne<Track>().WithMany().HasForeignKey(d => d.TrackId).OnDelete(DeleteBehavior.Restrict);
            // Quem criou pode excluir a conta sem apagar o placar de quem jogou.
            duel.HasOne<ApplicationUser>().WithMany().HasForeignKey(d => d.CreatorId).OnDelete(DeleteBehavior.SetNull);
            duel.HasMany(d => d.Questions).WithOne().HasForeignKey(q => q.DuelId).OnDelete(DeleteBehavior.Cascade);
            duel.HasMany(d => d.Players).WithOne().HasForeignKey(p => p.DuelId).OnDelete(DeleteBehavior.Cascade);
            duel.HasQueryFilter(d => d.TrackId == CurrentTrackId);
        });

        builder.Entity<DuelQuestion>(question =>
        {
            question.HasKey(q => new { q.DuelId, q.QuestionId });
            question.HasOne(q => q.Question).WithMany().HasForeignKey(q => q.QuestionId).OnDelete(DeleteBehavior.Restrict);
            question.HasQueryFilter(q => q.Question!.Subject!.TrackId == CurrentTrackId);
        });

        builder.Entity<DuelPlayer>(player =>
        {
            // Uma participação por pessoa em cada duelo.
            player.HasKey(p => new { p.DuelId, p.UserId });
            player.HasIndex(p => p.UserId);
            player.Ignore(p => p.HasFinished);
            player.Ignore(p => p.Elapsed);
            player.HasOne<ApplicationUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            player.HasMany(p => p.Answers).WithOne().HasForeignKey(a => new { a.DuelId, a.UserId }).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DuelAnswer>(answer =>
        {
            answer.HasKey(a => new { a.DuelId, a.UserId, a.QuestionId });
            answer.HasOne<QuestionOption>().WithMany().HasForeignKey(a => a.OptionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DailyQuestion>(daily =>
        {
            // Uma questão por dia em cada trilha, garantido pelo banco.
            daily.HasKey(d => new { d.TrackId, d.Date });
            daily.HasIndex(d => d.QuestionId);
            daily.HasOne<Track>().WithMany().HasForeignKey(d => d.TrackId).OnDelete(DeleteBehavior.Cascade);
            daily.HasOne<Question>().WithMany().HasForeignKey(d => d.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DiscursiveAnswer>(answer =>
        {
            answer.Property(a => a.Text).HasMaxLength(QuestionBank.DiscursiveAnswerMaxLength);
            answer.HasIndex(a => new { a.UserId, a.QuestionId });
            answer.HasOne<ApplicationUser>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            answer.HasOne<Question>().WithMany().HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
