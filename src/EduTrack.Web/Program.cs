using EduTrack.Application;
using EduTrack.Infrastructure;
using EduTrack.Persistence;
using System.Globalization;
using EduTrack.Web.Extensions;
using EduTrack.Web.Filters;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();
builder.Services.AddCookieAuth();
builder.Services.AddControllersWithViews(options => options.Filters.Add<FluentValidationFilter>());
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Keys live next to the app by default (/app/data-protection-keys inside the container image);
// override with DataProtection__KeysPath when the folder must be elsewhere.
var keysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "data-protection-keys");

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("EduTrack");

var app = builder.Build();


await app.MigrateAndSeedDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Turkish formatting for dates and numbers (e.g. "15 Ekim 2026").
var turkish = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(turkish),
    SupportedCultures = [turkish],
    SupportedUICultures = [turkish],
});

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Course}/{action=Index}/{id?}")
    .WithStaticAssets();


await app.RunAsync();
