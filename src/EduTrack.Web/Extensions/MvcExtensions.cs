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

        var demoContent = app.Configuration.GetValue<bool>("Seed:DemoContent");

        // The admin password comes from configuration (Admin__Password). Demo installs fall back to
        // the shared demo password so the README accounts work out of the box.
        var adminPassword = app.Configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(adminPassword) && demoContent)
            adminPassword = DemoDataSeeder.DemoPassword;

        await DbSeeder.SeedRolesAsync(context);

        if (string.IsNullOrWhiteSpace(adminPassword))
            app.Logger.LogWarning("Admin:Password is not set; the admin account was not created.");
        else
            await DbSeeder.SeedAdminUserAsync(context, adminPassword);

        if (demoContent)
            await DemoDataSeeder.SeedAsync(context);
    }
}
