using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.DTOs.File;

public record FileUploadRequest(Stream Content, string FileName, string? ContentType = null);
