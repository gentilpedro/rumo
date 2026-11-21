using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace EnadeAds.Accounts;

/// <summary>Um e-mail pronto para enviar: assunto, versão em HTML e versão em texto puro.</summary>
public sealed record AccountEmail(string Subject, string Html, string Text);

/// <summary>
/// Os e-mails da conta, com a identidade do Rumo e a assinatura da Aamora. O HTML é de e-mail: tabelas e estilo
/// em linha, que é o que Gmail e Outlook entendem. Tudo se lê sem as imagens, que muitos leitores bloqueiam, e
/// cada e-mail leva também a versão em texto puro.
///
/// Os links chegam das páginas de conta já escapados para HTML (padrão do Identity); aqui eles são lidos de volta
/// e escapados de novo no lugar certo.
/// </summary>
public static class AccountEmails
{
    // Cores do Rumo (DESIGN.md): azul-tinta, o laranja de destaque, texto e fios.
    private const string Ink = "#14213D";
    private const string InkText = "#E9EDF5";
    private const string Accent = "#C23D1C";
    private const string TextColor = "#1A2335";
    private const string Muted = "#556075";
    private const string Ground = "#F4F6F9";
    private const string Rule = "#D9DEE7";
    private const string Font = "Archivo, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif";

    // Mantém os acentos legíveis no HTML; continua escapando < > & " ' e caracteres de controle.
    private static readonly HtmlEncoder Html = HtmlEncoder.Create(UnicodeRanges.All);

    public const string AamoraUrl = "https://aamora.com.br";
    private const string AamoraLogo = "https://aamora.com.br/logo.png";

    public static AccountEmail Confirmation(string fullName, string encodedLink)
    {
        var link = WebUtility.HtmlDecode(encodedLink);
        var greeting = $"Olá, {FirstName(fullName)}!";
        return Build(
            subject: "Confirme seu e-mail no Rumo",
            preheader: "Falta um clique para começar a estudar.",
            title: "Confirme o seu e-mail",
            paragraphs:
            [
                greeting,
                "Falta um passo: confirme que este e-mail é seu. Com ele confirmado, você responde questões, salva o seu progresso e entra em duelos e salas de estudo.",
            ],
            button: ("Confirmar meu e-mail", link),
            code: null,
            closing: "Não foi você? É só ignorar este e-mail: sem a confirmação, nada acontece.",
            siteLink: link);
    }

    public static AccountEmail PasswordReset(string encodedLink)
    {
        var link = WebUtility.HtmlDecode(encodedLink);
        return Build(
            subject: "Crie uma nova senha no Rumo",
            preheader: "Use o botão para criar uma nova senha.",
            title: "Crie uma nova senha",
            paragraphs: ["Recebemos um pedido para redefinir a senha da sua conta no Rumo. Use o botão abaixo para criar uma nova."],
            button: ("Criar nova senha", link),
            code: null,
            closing: "Não pediu? Ignore este e-mail: a sua senha continua a mesma.",
            siteLink: link);
    }

    public static AccountEmail PasswordResetCode(string code) => Build(
        subject: "Seu código para redefinir a senha no Rumo",
        preheader: "Use este código para criar uma nova senha.",
        title: "Seu código para redefinir a senha",
        paragraphs: ["Recebemos um pedido para redefinir a senha da sua conta no Rumo. Digite este código na página de redefinição:"],
        button: null,
        code: code,
        closing: "Não pediu? Ignore este e-mail: a sua senha continua a mesma.",
        siteLink: null);

    /// <summary>"José Elson dos Santos Silva" vira "José": o e-mail fala com a pessoa, não lê o cadastro dela.</summary>
    public static string FirstName(string? fullName)
    {
        var first = (fullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrEmpty(first) ? "estudante" : first;
    }

    private static AccountEmail Build(string subject, string preheader, string title, string[] paragraphs,
        (string Label, string Url)? button, string? code, string closing, string? siteLink)
    {
        // O endereço do site sai do próprio link: cada instalação tem o seu domínio.
        var site = Uri.TryCreate(siteLink, UriKind.Absolute, out var uri) ? uri.GetLeftPart(UriPartial.Authority) : null;
        static string E(string value) => Html.Encode(value);

        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\">")
            .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">")
            .Append("<meta name=\"color-scheme\" content=\"light\">")
            .Append($"<title>{E(subject)}</title></head>")
            .Append($"<body style=\"margin:0;padding:0;background:{Ground};\">")
            // Texto que aparece ao lado do assunto na caixa de entrada.
            .Append($"<div style=\"display:none;max-height:0;overflow:hidden;opacity:0;color:{Ground};\">{E(preheader)}</div>")
            .Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:{Ground};\"><tr><td align=\"center\" style=\"padding:24px 12px;\">")
            .Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:560px;background:#FFFFFF;border:1px solid {Rule};border-radius:8px;overflow:hidden;font-family:{Font};\">");

        // Cabeçalho: o ícone e o nome do Rumo sobre o azul-tinta, com o fio laranja da marca.
        html.Append($"<tr><td style=\"background:{Ink};border-top:4px solid #FF8A65;padding:20px 28px;\">")
            .Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\"><tr>");
        if (site is not null)
        {
            html.Append($"<td style=\"padding-right:12px;\"><img src=\"{E(site)}/apple-touch-icon.png\" width=\"44\" height=\"44\" alt=\"\" style=\"display:block;border-radius:10px;\"></td>");
        }

        html.Append($"<td style=\"font-family:{Font};font-size:26px;line-height:1;font-weight:800;letter-spacing:-0.03em;color:{InkText};\">Rumo")
            .Append($"<div style=\"font-size:12px;line-height:1.4;font-weight:400;letter-spacing:0;color:#AEB8CF;padding-top:4px;\">Estude para a sua prova</div></td>")
            .Append("</tr></table></td></tr>");

        // Corpo.
        html.Append($"<tr><td style=\"padding:28px 28px 8px;font-family:{Font};color:{TextColor};\">")
            .Append($"<h1 style=\"margin:0 0 16px;font-size:22px;line-height:1.25;font-weight:700;color:{TextColor};\">{E(title)}</h1>");
        foreach (var paragraph in paragraphs)
        {
            html.Append($"<p style=\"margin:0 0 14px;font-size:16px;line-height:1.55;\">{E(paragraph)}</p>");
        }

        html.Append("</td></tr>");

        if (button is { } action)
        {
            html.Append("<tr><td style=\"padding:8px 28px 20px;\">")
                .Append($"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\"><tr><td style=\"background:{Accent};border-radius:6px;\">")
                .Append($"<a href=\"{E(action.Url)}\" style=\"display:inline-block;padding:14px 26px;font-family:{Font};font-size:16px;font-weight:700;color:#FFFFFF;text-decoration:none;\">{E(action.Label)}</a>")
                .Append("</td></tr></table></td></tr>")
                .Append($"<tr><td style=\"padding:0 28px 20px;font-family:{Font};font-size:13px;line-height:1.5;color:{Muted};\">")
                .Append("O botão não funcionou? Copie este endereço e cole no navegador:<br>")
                .Append($"<a href=\"{E(action.Url)}\" style=\"color:#1D4FA3;word-break:break-all;\">{E(action.Url)}</a></td></tr>");
        }

        if (code is not null)
        {
            html.Append("<tr><td style=\"padding:4px 28px 24px;\">")
                .Append($"<div style=\"display:inline-block;padding:14px 22px;background:{Ground};border:1px solid {Rule};border-radius:6px;font-family:Consolas,'Courier New',monospace;font-size:26px;font-weight:700;letter-spacing:0.12em;color:{TextColor};\">{E(code)}</div>")
                .Append("</td></tr>");
        }

        html.Append($"<tr><td style=\"padding:0 28px 28px;font-family:{Font};font-size:14px;line-height:1.55;color:{Muted};\">{E(closing)}</td></tr>");

        // Rodapé: de quem é a plataforma e por que a pessoa recebeu o e-mail.
        html.Append($"<tr><td style=\"padding:20px 28px;border-top:1px solid {Rule};background:#FAF7F2;font-family:{Font};font-size:13px;line-height:1.55;color:{Muted};\">")
            .Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\"><tr>")
            .Append($"<td style=\"padding-right:12px;vertical-align:top;\"><img src=\"{AamoraLogo}\" width=\"36\" height=\"36\" alt=\"\" style=\"display:block;border-radius:8px;\"></td>")
            .Append($"<td style=\"font-family:{Font};font-size:13px;line-height:1.55;color:{Muted};\">")
            .Append($"<strong style=\"color:{TextColor};\">Rumo</strong> é uma plataforma de estudos gratuita da <a href=\"{AamoraUrl}\" style=\"color:#1F3D6B;font-weight:700;text-decoration:none;\">Aamora</a>.<br>")
            .Append("Você recebeu este e-mail porque este endereço foi usado numa conta do Rumo. Não enviamos propaganda.");
        if (site is not null)
        {
            html.Append($"<br><a href=\"{E(site)}\" style=\"color:#1D4FA3;\">{E(new Uri(site).Host)}</a>");
        }

        html.Append("</td></tr></table></td></tr>")
            .Append("</table></td></tr></table></body></html>");

        var text = new StringBuilder();
        text.AppendLine("RUMO").AppendLine().AppendLine(title).AppendLine();
        foreach (var paragraph in paragraphs)
        {
            text.AppendLine(paragraph).AppendLine();
        }

        if (button is { } plain)
        {
            text.AppendLine($"{plain.Label}:").AppendLine(plain.Url).AppendLine();
        }

        if (code is not null)
        {
            text.AppendLine(code).AppendLine();
        }

        text.AppendLine(closing).AppendLine()
            .AppendLine("--")
            .AppendLine($"Rumo é uma plataforma de estudos gratuita da Aamora ({AamoraUrl}).")
            .AppendLine("Você recebeu este e-mail porque este endereço foi usado numa conta do Rumo. Não enviamos propaganda.");

        return new AccountEmail(subject, html.ToString(), text.ToString());
    }
}
