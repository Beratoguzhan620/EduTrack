
using Core.Persistence.Repository;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Persistence.Repositories;

public class StreamPostRepository(AppDbContext context) : BaseRepository<StreamPost, Guid, AppDbContext>(context), IStreamPostRepository
{

}
