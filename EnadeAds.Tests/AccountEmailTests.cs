using System.Text.Encodings.Web;
using EnadeAds.Accounts;

namespace EnadeAds.Tests;

public class AccountEmailTests
{
    // As páginas de conta mandam o link já escapado para HTML, como o Identity faz.
    private static readonly string Link = HtmlEncoder.Default.Encode("https://rumo.aamora.com.br/Account/ConfirmEmail?userId=abc&code=X1Y2");

    [Fact]
    public void Confirmation_HasTheBrandTheButtonAndTheLinkWrittenOut()
    {
        var email = AccountEmails.Confirmation("José Elson dos Santos Silva", Link);

        Assert.Equal("Confirme seu e-mail no Rumo", email.Subject);
        // Fala com a pessoa pelo primeiro nome, não lê o cadastro inteiro.
        Assert.Contains("Olá, José!", email.Html);
        Assert.DoesNotContain("Santos Silva", email.Html);
        Assert.Contains(">Confirmar meu e-mail</a>", email.Html);
        // O link aparece duas vezes, escapado uma vez só: no botão e por extenso.
        Assert.Equal(3, email.Html.Split("userId=abc&amp;code=X1Y2").Length - 1);
        Assert.DoesNotContain("&amp;amp;", email.Html);
        Assert.DoesNotContain("clique aqui", email.Html);
        Assert.Contains("https://rumo.aamora.com.br/apple-touch-icon.png", email.Html);
        Assert.Contains("plataforma de estudos gratuita da <a href=\"https://aamora.com.br\"", email.Html);
        Assert.Contains("Não foi você?", email.Html);
    }

    [Fact]
    public void EveryEmail_HasAPlainTextVersionWithTheRealLink()
    {
        var confirmation = AccountEmails.Confirmation("Ana", Link);
        var reset = AccountEmails.PasswordReset(Link);
        var code = AccountEmails.PasswordResetCode("482913");

        Assert.Contains("https://rumo.aamora.com.br/Account/ConfirmEmail?userId=abc&code=X1Y2", confirmation.Text);
        Assert.Contains("https://rumo.aamora.com.br/Account/ConfirmEmail?userId=abc&code=X1Y2", reset.Text);
        Assert.Contains("Criar nova senha", reset.Html);
        Assert.Contains("a sua senha continua a mesma", reset.Text);
        Assert.Contains("482913", code.Html);
        Assert.Contains("482913", code.Text);
        Assert.All(new[] { confirmation, reset, code }, email =>
        {
            Assert.DoesNotContain("<", email.Text);
            Assert.Contains("Aamora", email.Text);
            Assert.StartsWith("<!DOCTYPE html>", email.Html);
        });
    }

    [Theory]
    [InlineData("<script>alert(1)</script> Silva", "&lt;script&gt;alert(1)&lt;/script&gt;")]
    [InlineData("\"><img src=x onerror=alert(1)>", "&quot;&gt;&lt;img")]
    public void NameTypedByThePerson_NeverBecomesHtml(string name, string escaped)
    {
        var email = AccountEmails.Confirmation(name, Link);

        Assert.Contains(escaped, email.Html);
        Assert.DoesNotContain("<script>", email.Html);
        Assert.DoesNotContain("<img src=x", email.Html);
    }

    [Theory]
    [InlineData("Maria Clara Souza", "Maria")]
    [InlineData("  joão  ", "joão")]
    [InlineData("", "estudante")]
    [InlineData(null, "estudante")]
    public void FirstName_IsWhatTheGreetingUses(string? fullName, string expected)
    {
        Assert.Equal(expected, AccountEmails.FirstName(fullName));
    }
}
