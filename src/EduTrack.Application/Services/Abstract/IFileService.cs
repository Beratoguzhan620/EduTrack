using EduTrack.Application.DTOs.File;

namespace EduTrack.Application.Services.Abstract;

public interface IFileService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    Task<FileDeleteResult> DeleteAsync(FileDeleteRequest request, CancellationToken cancellationToken = default);
}
