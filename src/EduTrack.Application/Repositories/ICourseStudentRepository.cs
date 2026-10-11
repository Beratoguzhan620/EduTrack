using Core.Persistence.Repository;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.Repositories;

public interface ICourseStudentRepository : IRepository<CourseStudent, Guid>
{
}
