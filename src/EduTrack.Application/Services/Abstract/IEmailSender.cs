namespace EduTrack.Application.Services.Abstract;

public interface IEmailSender
{
    /// <summary>Sends an HTML e-mail. Returns false when e-mail delivery is not configured.</summary>
    Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
