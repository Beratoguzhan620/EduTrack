using Core.Persistence.Repository;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.Repositories;

public interface IStreamPostRepository : IRepository<StreamPost, Guid>
{
}

public interface ICourseMaterialRepository : IRepository<CourseMaterial, Guid>
{
}