
using Core.Persistence.Repository;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Persistence.Repositories;

public class CourseStudentRepository(AppDbContext context) : BaseRepository<CourseStudent, Guid, AppDbContext>(context), ICourseStudentRepository
{

}
