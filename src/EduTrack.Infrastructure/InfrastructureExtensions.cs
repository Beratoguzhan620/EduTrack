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

        services.AddHttpClient<IFileService, FileApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        });



        return services;
    }

}
