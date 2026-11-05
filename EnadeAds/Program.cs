using EnadeAds.Accounts;
using EnadeAds.Achievements;
using EnadeAds.Blog;
using EnadeAds.Challenges;
using EnadeAds.Components;
using EnadeAds.Components.Account;
using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.MockExams;
using EnadeAds.Performance;
using EnadeAds.Privacy;
using EnadeAds.Questions;
using EnadeAds.Security;
using EnadeAds.Study;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando EnadeAds");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents()
        .AddAuthenticationStateSerialization();

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;

            // 5 senhas erradas seguidas bloqueiam a conta por 15 minutos (força bruta em uma conta específica).
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddRoles<IdentityRole>()
        .AddErrorDescriber<PortugueseIdentityErrorDescriber>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    // Revalida a sessão a cada minuto: mudança de perfil (aprovar ou revogar professor) vale rápido.
    builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(1));
    builder.Services.AddScoped<TeacherApprovals>();
    builder.Services.AddScoped<AccountAdmin>();
    builder.Services.AddScoped<RoadmapService>();
    builder.Services.AddScoped<RoadmapEditor>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<TrackScope>();
    builder.Services.AddScoped<TrackCatalog>();
    builder.Services.AddScoped<ExamSettings>();
    builder.Services.AddScoped<EnadeAds.Reviews.ReviewQueue>();
    builder.Services.AddScoped<EnadeAds.Reviews.ReviewDecisions>();
    builder.Services.AddScoped<SemesterPlan>();
    builder.Services.AddScoped<SubjectCatalog>();
    builder.Services.AddScoped<QuestionBank>();
    builder.Services.AddScoped<QuestionEditor>();
    builder.Services.AddScoped<InepQuestionImporter>();
    builder.Services.AddScoped<ChallengeService>();
    builder.Services.AddScoped<BlogService>();
    builder.Services.AddScoped<PerformanceService>();
    builder.Services.AddScoped<MockExamService>();
    builder.Services.AddScoped<DuelService>();
    builder.Services.AddScoped<AchievementService>();
    builder.Services.AddSingleton<LocalCalendar>();
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.Configure<ExamOptions>(builder.Configuration.GetSection(ExamOptions.SectionName));

    // Limite de tentativas por IP nos formulários de conta (força bruta e cadastro em massa).
    builder.Services.AddAccountRateLimit(builder.Configuration);

    // Atrás do Nginx: o IP do aluno vem no X-Forwarded-For, aceito só dos proxies listados em ReverseProxy:KnownNetworks.
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        foreach (var network in builder.Configuration.GetSection("ReverseProxy:KnownNetworks").Get<string[]>() ?? [])
        {
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        }
    });

    // /health: o deploy só dá a versão como no ar quando ele responde (e volta a anterior se não responder).
    builder.Services.AddHealthChecks();

    // Em produção as chaves que protegem o cookie de login e os formulários ficam num volume
    // (DataProtection:KeysPath): sem isso, cada deploy tiraria todo mundo da conta.
    if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } keysPath)
    {
        builder.Services.AddDataProtection()
            .SetApplicationName("Rumo")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
    }

    builder.Services.AddScoped<PersonalDataExport>();
    builder.Services.Configure<PrivacyOptions>(builder.Configuration.GetSection(PrivacyOptions.SectionName));
    builder.Services.Configure<AnalyticsOptions>(builder.Configuration.GetSection(AnalyticsOptions.SectionName));

    builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
    builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));

    var smtp = builder.Configuration.GetSection(SmtpOptions.SectionName).Get<SmtpOptions>() ?? new SmtpOptions();
    if (smtp.IsConfigured)
    {
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, SmtpEmailSender>();
    }
    else if (builder.Environment.IsDevelopment())
    {
        // Sem SMTP em desenvolvimento, o link de confirmação aparece na própria tela.
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
    }
    else
    {
        // Sem SMTP fora de desenvolvimento o site sobe, mas nenhum e-mail sai e o link não aparece na tela:
        // mostrá-lo permitiria confirmar qualquer e-mail .edu. Contas novas esperam o SMTP.
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, UnconfiguredEmailSender>();
    }

    var app = builder.Build();

    await DatabaseInitializer.InitializeAsync(app.Services);

    app.UseForwardedHeaders();
    var analytics = builder.Configuration.GetSection(AnalyticsOptions.SectionName).Get<AnalyticsOptions>() ?? new AnalyticsOptions();
    app.UseSecurityHeaders(analytics.ValidOrigin);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();
    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

    app.UseRateLimiter();
    app.UseAntiforgery();
    app.UseTrackChoice();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(EnadeAds.Client._Imports).Assembly);

    // Add additional endpoints required by the Identity /Account Razor components.
    app.MapAdditionalIdentityEndpoints();
    app.MapBlogFeed();
    app.MapTrackSwitch();

    // As migrations rodam antes do app começar a atender (DatabaseInitializer), então /health só
    // responde com o banco atualizado.
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "EnadeAds encerrado de forma inesperada");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
