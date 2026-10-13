using System.Net;
using System.Net.Mail;
using EduTrack.Application.DTOs.Settings;
using EduTrack.Application.Services.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduTrack.Infrastructure.Integrations;

/// <summary>
/// Sends e-mail over SMTP. When no SMTP host is configured (typical for local development)
/// the message is written to the log instead, so flows such as e-mail verification keep working.
/// </summary>
public class SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        if (!settings.IsConfigured)
        {
            logger.LogWarning("SMTP is not configured; e-mail to {To} with subject '{Subject}' was not sent. Body: {Body}", to, subject, htmlBody);
            return false;
        }

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            Credentials = new NetworkCredential(settings.Username, settings.Password),
        };

        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(to);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (SmtpException ex)
        {
            logger.LogError(ex, "Sending e-mail to {To} failed.", to);
            return false;
        }
    }
}
