using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using EnadeAds.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EnadeAds.Accounts;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    private readonly SmtpOptions smtp = options.Value;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsync(email, AccountEmails.Confirmation(user.FullName, confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsync(email, AccountEmails.PasswordReset(resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, AccountEmails.PasswordResetCode(resetCode));

    private async Task SendAsync(string to, AccountEmail email)
    {
        var subject = email.Subject;
        using var message = new MailMessage
        {
            From = new MailAddress(smtp.FromAddress, smtp.FromName),
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            // O corpo é o texto puro, e o HTML vai como alternativa: quem não mostra HTML lê o texto, e o e-mail
            // com as duas versões passa melhor pelos filtros de spam.
            Body = email.Text,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = false,
        };
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(email.Html, Encoding.UTF8, MediaTypeNames.Text.Html));
        message.To.Add(to);

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Credentials = string.IsNullOrEmpty(smtp.UserName) ? null : new NetworkCredential(smtp.UserName, smtp.Password),
        };

        await client.SendMailAsync(message);
        logger.LogInformation("E-mail '{Subject}' enviado", subject);
    }
}
