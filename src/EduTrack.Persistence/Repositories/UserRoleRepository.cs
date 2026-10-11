
using Core.Persistence.Repository;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Persistence.Repositories;

public class UserRoleRepository(AppDbContext context) : BaseRepository<UserRole, Guid, AppDbContext>(context), IUserRoleRepository
{

}
