namespace EnadeAds.Security;

/// <summary>
/// Contagem de visitas da instalação (ex.: Umami). O script é inserido pelo proxy reverso; o app só precisa liberar a origem
/// dele na CSP e contar isso na política de privacidade. Sem <see cref="Origin"/>, nada disso aparece.
/// </summary>
public sealed class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    /// <summary>Origem do serviço de contagem, só esquema e host (ex.: <c>https://stats.aamora.com.br</c>).</summary>
    public string Origin { get; set; } = "";

    /// <summary>
    /// A origem normalizada, ou nula se não houver ou for inválida. Só aceita HTTPS sem caminho: o valor entra na CSP,
    /// e uma configuração errada não pode abrir a política (ex.: <c>https:</c> ou <c>*</c>).
    /// </summary>
    public string? ValidOrigin =>
        Uri.TryCreate(Origin.Trim(), UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            && uri.AbsolutePath == "/" && string.IsNullOrEmpty(uri.Query) && !uri.Host.Contains('*')
            ? uri.GetLeftPart(UriPartial.Authority)
            : null;
}
