using EduTrack.Application.DTOs.Settings;
using EduTrack.Application.Services.Abstract;
using EduTrack.Infrastructure.Integrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduTrack.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        string baseUrl = configuration["FileApi:BaseUrl"] ?? throw new InvalidOperationException("FileApi:BaseUrl is not configured.");
        string apiKey = configuration["FileApi:ApiKey"] ?? throw new InvalidOperationException("FileApi:ApiKey is not configured.");

        // Links stored for uploaded files must work in the browser, which may not see the internal address.
        string publicUrl = configuration["FileApi:PublicUrl"] is { Length: > 0 } configured ? configured : baseUrl;
        services.AddSingleton(new FileApiPublicUrl(new Uri(EnsureTrailingSlash(publicUrl))));

        services.AddHttpClient<IFileService, FileApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        });

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }

    private static string EnsureTrailingSlash(string url) => url.EndsWith('/') ? url : url + "/";
}
