namespace EduTrack.Application.DTOs.File;

public record FileUploadResult(string FileId, string OriginalFileName, long Length, string OpenUrl, string DownloadUrl);
