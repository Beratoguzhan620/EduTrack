using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace FileAPI.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController(IWebHostEnvironment environment, IConfiguration configuration) : ControllerBase
{
    private const long MaximumFileSize = 100 * 1024 * 1024;
    private const int BufferSize = 81920;
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
    private readonly string _storagePath = Path.GetFullPath(
        configuration["FileServer:StoragePath"] ?? "FileStorage",
        environment.ContentRootPath);

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumFileSize)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return BadRequest("Yüklenecek dosya boş olamaz.");
        }

        if (file.Length > MaximumFileSize)
        {
            return BadRequest($"Dosya boyutu en fazla {MaximumFileSize / 1024 / 1024} MB olabilir.");
        }

        var originalFileName = Path.GetFileName(file.FileName);
        if (!IsValidFileName(originalFileName))
        {
            return BadRequest("Geçersiz dosya adı.");
        }

        Directory.CreateDirectory(_storagePath);
        var fileId = $"{Guid.NewGuid():N}{Path.GetExtension(originalFileName).ToLowerInvariant()}";
        var filePath = GetFilePath(fileId);

        await using var output = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            BufferSize,
            FileOptions.Asynchronous);
        await file.CopyToAsync(output, cancellationToken);

        return CreatedAtAction(nameof(Open), new { fileId }, new
        {
            fileId,
            originalFileName,
            file.Length,
            openUrl = Url.Action(nameof(Open), new { fileId }),
            downloadUrl = Url.Action(nameof(Download), new { fileId })
        });
    }

    [HttpGet("{fileId}/download")]
    public IActionResult Download(string fileId)
    {
        if (!TryGetExistingFile(fileId, out var filePath))
        {
            return NotFound();
        }

        return PhysicalFile(filePath, GetContentType(fileId), fileId, enableRangeProcessing: true);
    }

    [HttpGet("{fileId}/open", Name = nameof(Open))]
    public IActionResult Open(string fileId)
    {
        if (!TryGetExistingFile(fileId, out var filePath))
        {
            return NotFound();
        }

        return PhysicalFile(filePath, GetContentType(fileId), enableRangeProcessing: true);
    }

    [HttpDelete("{fileId}")]
    public IActionResult Delete(string fileId)
    {
        if (!TryGetExistingFile(fileId, out var filePath))
        {
            return NotFound();
        }

        System.IO.File.Delete(filePath);
        return NoContent();
    }

    private bool TryGetExistingFile(string fileId, out string filePath)
    {
        filePath = string.Empty;
        if (!IsValidFileId(fileId))
        {
            return false;
        }

        filePath = GetFilePath(fileId);
        return System.IO.File.Exists(filePath);
    }

    private string GetFilePath(string fileName) => Path.Combine(_storagePath, fileName);

    private static bool IsValidFileName(string fileName) =>
        !string.IsNullOrWhiteSpace(fileName) &&
        fileName is not "." and not ".." &&
        fileName == Path.GetFileName(fileName) &&
        fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    private static bool IsValidFileId(string fileId)
    {
        if (!IsValidFileName(fileId))
        {
            return false;
        }

        return Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileId), "N", out _);
    }

    private static string GetContentType(string fileName) =>
        ContentTypeProvider.TryGetContentType(fileName, out var contentType)
            ? contentType
            : "application/octet-stream";
}
