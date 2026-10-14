using EduTrack.Persistence;
using EduTrack.Persistence.Seeders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Web.Extensions;

public static class MvcExtensions
{
    public static void AddCookieAuth(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                 .AddCookie(options =>
                 {
                     options.Cookie.Name = "auth-cookie";
                     options.Cookie.HttpOnly = true;
                     options.Cookie.IsEssential = true;

                     options.LoginPath = "/Login";
                     options.LogoutPath = "/Logout";
                     options.AccessDeniedPath = "/access-denied";

                     options.ExpireTimeSpan = TimeSpan.FromHours(8);
                     options.SlidingExpiration = true;
                 });
    }

    public static async Task MigrateAndSeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        await DbSeeder.SeedAsync(context);

        if (app.Configuration.GetValue<bool>("Seed:DemoContent"))
            await DemoDataSeeder.SeedAsync(context);

    }
}
