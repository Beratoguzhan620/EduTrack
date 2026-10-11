
using Core.Persistence.Repository;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User, Guid, AppDbContext>(context), IUserRepository
{

    public async Task<User?> GetUserWithRolesByEmailAsync(string email)
    {
        var user = await context
            .Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        return user;
      
    }


}
