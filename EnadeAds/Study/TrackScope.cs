using EnadeAds.Data;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Study;

/// <summary>
/// A trilha ativa de quem está navegando, guardada num cookie. O <see cref="ApplicationDbContext"/> filtra
/// roadmap, questões, desafios e simulados por ela. Fora de uma requisição (importação na subida) vale a
/// trilha padrão.
///
/// Quem ainda não escolheu nenhuma prova não fica numa trilha por omissão: o início mostra a escolha e as
/// seções de estudo mandam para lá (<see cref="TrackEndpoints.UseTrackChoice"/>). A configuração
/// <c>Tracks:Default</c> (slug) dispensa a escolha; os testes usam isso.
/// </summary>
public sealed class TrackScope(IHttpContextAccessor http, IConfiguration configuration)
{
    public const string CookieName = "trilha";

    private int? id;
    private bool chosen;

    private int? Chosen => Tracks.IdOf(http.HttpContext?.Request.Cookies[CookieName]) ?? Tracks.IdOf(configuration["Tracks:Default"]);

    public int Id => id ??= Chosen ?? Tracks.Default;

    /// <summary>Se a pessoa já escolheu uma prova (ou a configuração escolheu por ela).</summary>
    public bool HasChosen => chosen || Chosen is not null;

    /// <summary>Troca a trilha só nesta requisição (testes e tarefas de fundo).</summary>
    public void Use(int trackId)
    {
        id = trackId;
        chosen = true;
    }
}

/// <summary>
/// Trilha ativa com os dados do banco (nome e datas que o admin pode mudar).
/// Carrega as trilhas uma vez por requisição num contexto próprio: o cabeçalho e a página pedem a trilha ao mesmo
/// tempo, e um <see cref="ApplicationDbContext"/> não aceita duas consultas simultâneas.
/// </summary>
public sealed class TrackCatalog(IServiceScopeFactory scopes, TrackScope scope)
{
    private Task<IReadOnlyList<Track>>? all;

    public async Task<Track> CurrentAsync(CancellationToken cancellationToken = default)
    {
        var tracks = await ListAsync(cancellationToken);
        return tracks.FirstOrDefault(t => t.Id == scope.Id) ?? tracks.FirstOrDefault(t => t.Id == Tracks.Default)
            ?? Tracks.Seed.Single(t => t.Id == Tracks.Default);
    }

    public Task<IReadOnlyList<Track>> ListAsync(CancellationToken cancellationToken = default) => all ??= LoadAsync(cancellationToken);

    private async Task<IReadOnlyList<Track>> LoadAsync(CancellationToken cancellationToken)
    {
        using var own = scopes.CreateScope();
        var db = own.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Tracks.AsNoTracking().OrderBy(t => t.Order).ToListAsync(cancellationToken);
    }
}

public static class TrackEndpoints
{
    /// <summary>
    /// <c>/trilha/{slug}?volta=/questoes</c> troca a trilha ativa e volta para a página. É um link comum (GET)
    /// porque só muda uma preferência de navegação, como trocar de idioma, e funciona sem JavaScript.
    /// </summary>
    public static void MapTrackSwitch(this IEndpointRouteBuilder app) =>
        app.MapGet("/trilha/{slug}", (string slug, string? volta, HttpContext context) =>
        {
            if (Tracks.IdOf(slug) is null)
            {
                return Results.NotFound();
            }

            context.Response.Cookies.Append(TrackScope.CookieName, slug, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                MaxAge = TimeSpan.FromDays(365),
            });

            // Só volta para páginas do próprio site; o resto vai para o início.
            return Results.LocalRedirect(IsLocal(volta) ? volta! : "/");
        });

    /// <summary>Seções que só fazem sentido dentro de uma prova (primeiro trecho do caminho).</summary>
    private static readonly HashSet<string> TrackSections = new(StringComparer.OrdinalIgnoreCase)
    {
        "roadmap", "materias", "questoes", "desafios", "simulados", "meu-desempenho", "sobrevivencia",
    };

    /// <summary>
    /// Quem abre uma seção de estudo sem ter escolhido a prova vai para o início, que pede a escolha, e depois
    /// volta para onde ia. Só GET: formulários vêm de páginas que já passaram por aqui.
    /// </summary>
    public static void UseTrackChoice(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? "/";
            var section = path.TrimStart('/').Split('/')[0];
            if (HttpMethods.IsGet(context.Request.Method) && TrackSections.Contains(section)
                && !context.RequestServices.GetRequiredService<TrackScope>().HasChosen)
            {
                context.Response.Redirect("/?volta=" + Uri.EscapeDataString(path + context.Request.QueryString));
                return;
            }

            await next();
        });

    public static bool IsLocal(string? url) =>
        !string.IsNullOrEmpty(url) && url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
}
