namespace EnadeAds.Privacy;

public sealed class PrivacyOptions
{
    public const string SectionName = "Privacy";

    /// <summary>Versão dos termos e da política. Mudou o texto, muda a versão (o aceite guardado indica qual foi lido).</summary>
    public const string TermsVersion = "2026-11-05";

    /// <summary>Quem responde pelos dados nesta instalação (o projeto é aberto: cada instalação tem o seu responsável).</summary>
    public string ControllerName { get; set; } = "";

    /// <summary>E-mail para pedidos sobre dados pessoais (acesso, correção, exclusão).</summary>
    public string ContactEmail { get; set; } = "";
}
