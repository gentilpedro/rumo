using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace EnadeAds.Questions;

public sealed record StatementBlock(string Text, bool IsCode)
{
    /// <summary>Bloco em que toda linha começa com "- ": vira lista.</summary>
    public bool IsList => !IsCode && Text.Split('\n').All(line => line.StartsWith("- ", StringComparison.Ordinal));
}

/// <summary>
/// Converte o enunciado (texto com marcação mínima) em HTML seguro. Todo texto é escapado antes de
/// receber as poucas tags permitidas, então nenhum HTML digitado pelo professor chega à página.
///
/// Marcação aceita:
/// - linha em branco separa parágrafos;
/// - bloco entre linhas <c>```</c> vira código pré-formatado;
/// - bloco em que toda linha começa com <c>- </c> vira lista;
/// - <c>**negrito**</c>, <c>`código`</c>, <c>[texto](https://…)</c> e <c>![descrição](https://…)</c> dentro de parágrafos;
/// - sem cercas, blocos com cara de código (indentação, tabelas, chaves, <c>;</c>) também viram código,
///   para manter o conteúdo antigo legível. Uma lista nunca é código, mesmo com itens terminados em <c>;</c>,
///   e <c> | </c> só indica tabela em bloco de mais de uma linha (numa frase, costuma separar versos).
/// </summary>
public static partial class StatementFormatter
{
    private const string Fence = "```";

    // Mantém acentos legíveis no HTML; continua escapando < > & " ' e caracteres de controle.
    private static readonly HtmlEncoder Html = HtmlEncoder.Create(UnicodeRanges.All);

    public static IReadOnlyList<StatementBlock> Split(string statement)
    {
        var blocks = new List<StatementBlock>();
        var current = new List<string>();
        var inFence = false;

        foreach (var rawLine in statement.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.TrimEnd();

            if (line.TrimStart().StartsWith(Fence, StringComparison.Ordinal))
            {
                if (inFence)
                {
                    blocks.Add(new StatementBlock(string.Join('\n', current), IsCode: true));
                    current.Clear();
                }
                else
                {
                    Flush();
                }

                inFence = !inFence;
                continue;
            }

            if (inFence)
            {
                current.Add(line);
            }
            else if (line.Length == 0)
            {
                Flush();
            }
            else
            {
                current.Add(line);
            }
        }

        // Cerca aberta e não fechada: trata o resto como código em vez de perder o texto.
        if (inFence)
        {
            blocks.Add(new StatementBlock(string.Join('\n', current), IsCode: true));
        }
        else
        {
            Flush();
        }

        return blocks;

        void Flush()
        {
            if (current.Count == 0)
            {
                return;
            }

            var isList = current.All(line => line.StartsWith("- ", StringComparison.Ordinal));
            var isTable = current.Count > 1 && current.Any(line => line.Contains(" | ", StringComparison.Ordinal));
            blocks.Add(new StatementBlock(string.Join('\n', current), !isList && (isTable || current.Any(LooksLikeCode))));
            current.Clear();
        }
    }

    public static string ToHtml(string statement)
    {
        var html = new StringBuilder();
        foreach (var block in Split(statement))
        {
            if (block.IsCode)
            {
                html.Append("<pre><code>").Append(Html.Encode(block.Text)).Append("</code></pre>");
            }
            else if (block.IsList)
            {
                html.Append("<ul>");
                foreach (var line in block.Text.Split('\n'))
                {
                    html.Append("<li>").Append(Inline(line[2..])).Append("</li>");
                }

                html.Append("</ul>");
            }
            else
            {
                html.Append("<p>").Append(Inline(block.Text)).Append("</p>");
            }
        }

        return html.ToString();
    }

    private static string Inline(string text)
    {
        var html = new StringBuilder();
        var position = 0;

        foreach (Match match in InlineToken().Matches(text))
        {
            html.Append(Encode(text[position..match.Index]));

            if (match.Groups["bold"].Success)
            {
                html.Append("<strong>").Append(Encode(match.Groups["bold"].Value)).Append("</strong>");
            }
            else if (match.Groups["code"].Success)
            {
                html.Append("<code>").Append(Encode(match.Groups["code"].Value)).Append("</code>");
            }
            else if (match.Groups["href"].Success)
            {
                if (TryHttps(match.Groups["href"].Value, out var link))
                {
                    html.Append("<a href=\"").Append(Html.Encode(link.AbsoluteUri))
                        .Append("\" target=\"_blank\" rel=\"noopener noreferrer nofollow\">")
                        .Append(Encode(match.Groups["label"].Value)).Append("</a>");
                }
                else
                {
                    // Link sem https (javascript:, data:, http) aparece como texto.
                    html.Append(Encode(match.Value));
                }
            }
            else if (TryImageSource(match.Groups["src"].Value, out var source))
            {
                html.Append("<img src=\"").Append(Html.Encode(source))
                    .Append("\" alt=\"").Append(Html.Encode(match.Groups["alt"].Value))
                    .Append("\" loading=\"lazy\" class=\"img-fluid my-2\" />");
            }
            else
            {
                // Imagem sem https (ou com URL inválida) aparece como texto, sem virar tag.
                html.Append(Encode(match.Value));
            }

            position = match.Index + match.Length;
        }

        html.Append(Encode(text[position..]));
        return html.ToString();
    }

    /// <summary>Imagem por https ou do próprio site, dentro de img/ (ex.: figuras das provas em img/inep/2021/q03-1.png).</summary>
    private static bool TryImageSource(string value, out string source)
    {
        if (TryHttps(value, out var uri))
        {
            source = uri.AbsoluteUri;
            return true;
        }

        source = value;
        return value.StartsWith("img/", StringComparison.Ordinal)
            && !value.Contains("..", StringComparison.Ordinal)
            && !value.Contains("//", StringComparison.Ordinal)
            && !value.Contains(':')
            && !value.Contains('\\');
    }

    private static bool TryHttps(string value, out Uri uri) =>
        Uri.TryCreate(value, UriKind.Absolute, out uri!) && uri.Scheme == Uri.UriSchemeHttps;

    private static string Encode(string text) => Html.Encode(text).Replace("&#xA;", "<br />");

    private static bool LooksLikeCode(string line) =>
        line.StartsWith("  ", StringComparison.Ordinal)
        || line.Contains("  ", StringComparison.Ordinal)
        || line.Contains('{') || line.Contains('}')
        || line.TrimEnd().EndsWith(';');

    [GeneratedRegex(@"\*\*(?<bold>[^*\n]+)\*\*|`(?<code>[^`\n]+)`|!\[(?<alt>[^\]\n]*)\]\((?<src>[^)\s]+)\)|\[(?<label>[^\]\n]+)\]\((?<href>[^)\s]+)\)")]
    private static partial Regex InlineToken();
}
