using EduTrack.Application.DTOs.File;
using EduTrack.Application.Services.Abstract;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EduTrack.Infrastructure.Integrations;

/// <summary>Talks to external-services/FileAPI.</summary>
/// <param name="publicBaseUrl">
/// Address browsers use to reach the File API. It differs from the HttpClient base address when the
/// app talks to the File API over an internal network (for example <c>http://fileapi:8080</c> in Docker).
/// </param>
public class FileApiClient(HttpClient httpClient, FileApiPublicUrl publicBaseUrl) : IFileService
{
    private readonly string Endpoint = "api/files";
    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request.Content);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);

        var fileContent = new StreamContent(request.Content);

        if (!string.IsNullOrWhiteSpace(request.ContentType))
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);


        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", request.FileName);

        using var response = await httpClient.PostAsync(Endpoint, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<FileUploadResult>(cancellationToken)
            ?? throw new HttpRequestException("File API returned an empty upload response.");

        // The File API answers with paths such as /api/files/{id}; store links the browser can open.
        return result with
        {
            OpenUrl = publicBaseUrl.Resolve(result.OpenUrl),
            DownloadUrl = publicBaseUrl.Resolve(result.DownloadUrl),
        };
    }


    public async Task<FileDeleteResult> DeleteAsync(FileDeleteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);

        using var response = await httpClient.DeleteAsync($"{Endpoint}/{request.FileName}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return new FileDeleteResult(true);

    }

}

public sealed record FileApiPublicUrl(Uri BaseUri)
{
    public string Resolve(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var absolute) && absolute.Scheme is "http" or "https"
            ? url
            : new Uri(BaseUri, url.TrimStart('/')).ToString();
}
