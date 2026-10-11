using EduTrack.Application.DTOs.File;
using EduTrack.Application.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Infrastructure.Integrations;

public class S3FileService : IFileService
{
    public Task<FileDeleteResult> DeleteAsync(FileDeleteRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
