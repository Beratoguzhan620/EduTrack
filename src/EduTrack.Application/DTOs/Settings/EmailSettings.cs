namespace EduTrack.Application.DTOs.Settings;

/// <summary>
/// SMTP settings, bound from the "Email" configuration section. Credentials come from
/// User Secrets or environment variables (Email__Host, Email__Username, Email__Password, ...).
/// When <see cref="Host"/> is empty no e-mail is sent and the content is written to the log.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "EduTrack";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(FromAddress);
}
