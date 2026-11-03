namespace EnadeAds.Security;

/// <summary>Cabeçalhos de segurança em todas as respostas.</summary>
public static class SecurityHeaders
{
    /// <summary>
    /// CSP da aplicação. Scripts só do próprio site; <c>'unsafe-inline'</c> continua necessário porque o Blazor
    /// gera um import map inline sem nonce, e <c>'wasm-unsafe-eval'</c> porque os componentes WebAssembly rodam no navegador.
    /// A defesa principal contra XSS é o escape de todo texto (StatementFormatter e Blazor), coberto por testes.
    /// A contagem de visitas (<see cref="AnalyticsOptions"/>), quando configurada, é a única origem externa de script e de conexão.
    /// </summary>
    public static string ContentSecurityPolicyFor(string? analyticsOrigin)
    {
        var analytics = analyticsOrigin is null ? "" : " " + analyticsOrigin;
        return "default-src 'self'; " +
            $"script-src 'self' 'unsafe-inline' 'wasm-unsafe-eval'{analytics}; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' https: data:; " +
            "font-src 'self'; " +
            $"connect-src 'self'{analytics}; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'; " +
            "frame-ancestors 'none'";
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, string? analyticsOrigin)
    {
        var policy = ContentSecurityPolicyFor(analyticsOrigin);
        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.ContentSecurityPolicy = policy;
                headers.XContentTypeOptions = "nosniff";
                headers.XFrameOptions = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
                return Task.CompletedTask;
            });

            await next();
        });
    }
}
