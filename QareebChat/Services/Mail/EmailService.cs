using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using QareebChat.Services.EmailTemplates;
using QareebChat.Settings;

namespace QareebChat.Services.Mail;

public class EmailService(ILogger<EmailService> logger, IOptions<MailSettings> mailSettings, IConfiguration configuration) : IEmailSender
{
    private readonly MailSettings _mailConfig = mailSettings.Value;
    private readonly ILogger<EmailService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.Sender = MailboxAddress.Parse(_mailConfig.Mail);
        message.Subject = subject;
        message.To.Add(MailboxAddress.Parse(email));

        var builder = new BodyBuilder { HtmlBody = htmlMessage };
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(_mailConfig.Host, _mailConfig.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailConfig.Mail, _mailConfig.Password);
        await smtp.SendAsync(message);
        _logger.LogInformation("Email sent to {Email}", email);
        await smtp.DisconnectAsync(true);
    }

   
}
