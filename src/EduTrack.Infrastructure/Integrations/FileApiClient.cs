using EduTrack.Application.DTOs.File;
using EduTrack.Application.Services.Abstract;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EduTrack.Infrastructure.Integrations;

public class FileApiClient(HttpClient httpClient) : IFileService
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

        var result = await response.Content.ReadFromJsonAsync<FileUploadResult>(cancellationToken);
        return result;
    }


    public async Task<FileDeleteResult> DeleteAsync(FileDeleteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);

        using var response = await httpClient.DeleteAsync($"{Endpoint}/{request.FileName}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return new FileDeleteResult(true);

    }

}
