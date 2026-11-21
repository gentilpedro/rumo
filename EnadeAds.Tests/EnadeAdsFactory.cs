using EnadeAds.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnadeAds.Tests;

/// <summary>Sobe a aplicação com SQLite em memória no lugar do PostgreSQL.</summary>
public sealed class EnadeAdsFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@exemplo.edu.br";

    private readonly SqliteConnection connection = new("DataSource=:memory:");

    public EnadeAdsFactory()
    {
        connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=nao-usado");
        builder.UseSetting("Admin:Emails:0", AdminEmail);
        // Quase todos os testes são da trilha original; os de escolha de prova usam um host sem isso.
        builder.UseSetting("Tracks:Default", "enade-ads");

        // Os testes fazem centenas de logins pelo mesmo "IP"; o limite em si é testado direto em AccountRateLimitTests.
        builder.UseSetting("RateLimit:AccountPostsPerMinute", "100000");

        // O desafio da semana automático é publicado por um serviço de fundo; nos testes, só quando o teste pede.
        builder.UseSetting("Challenges:AutoWeekly", "false");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection).AddInterceptors(new AsyncDatabaseInterceptor()));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            connection.Dispose();
        }
    }
}
