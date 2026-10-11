using Core.Persistence.Repository;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetUserWithRolesByEmailAsync(string email);
}
