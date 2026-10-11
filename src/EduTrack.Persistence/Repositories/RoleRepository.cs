
using Core.Persistence.Repository;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Persistence.Repositories;

public class RoleRepository(AppDbContext context) : BaseRepository<Role, int, AppDbContext>(context), IRoleRepository
{

}
