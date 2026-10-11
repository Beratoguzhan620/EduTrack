using System.Security.Cryptography;
using System.Text;

namespace FileAPI.Middleware;

public class FileApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string HeaderName = "X-Api-Key";
    private readonly byte[] _apiKeyHash = CreateConfiguredKeyHash(configuration);

    public async Task InvokeAsync(HttpContext context)
    {
        if (RequiresApiKey(context) && !HasValidApiKey(context))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Geçersiz veya eksik API anahtarı." });
            return;
        }

        await next(context);
    }

    private static bool RequiresApiKey(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/api/files") &&
        (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method));

    private bool HasValidApiKey(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var suppliedKey) || suppliedKey.Count != 1)
        {
            return false;
        }

        var suppliedKeyHash = SHA256.HashData(Encoding.UTF8.GetBytes(suppliedKey.ToString()));
        return CryptographicOperations.FixedTimeEquals(suppliedKeyHash, _apiKeyHash);
    }

    private static byte[] CreateConfiguredKeyHash(IConfiguration configuration)
    {
        var apiKey = configuration["FileServer:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("FileServer:ApiKey yapılandırılmalıdır.");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
    }
}
