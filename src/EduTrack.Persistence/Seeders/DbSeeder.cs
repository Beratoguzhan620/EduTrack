using Core.Security;
using EduTrack.Domain.Constants;
using EduTrack.Domain.Entities;
using EduTrack.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Persistence.Seeders;

public static class DbSeeder
{

    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedRolesAsync(context);
        await SeedAdminUserAsync(context);
    }

    public static async Task SeedRolesAsync(AppDbContext context)
    {
        var baseRoles = RoleConstants.BaseRoles;

        foreach (var role in baseRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Id == role.Id))
            {
                await context.Roles.AddAsync(role);
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedAdminUserAsync(AppDbContext context)
    {
        var passwordResult = HashingHelper.CreatePasswordHash("112");
        var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var adminUser = new User
        {
            Id = adminId,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            IsActive = true,
            EmailConfirmed = true,
            PasswordHash = passwordResult.Hash,
            PasswordSalt = passwordResult.Salt,
            UserRoles = [new UserRole { RoleId = RoleConstants.AdminId }]
        };

        if (!await context.Users.AnyAsync(u => u.Id == adminId))
        {
            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
