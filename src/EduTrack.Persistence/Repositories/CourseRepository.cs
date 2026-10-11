
using Core.Persistence.Repository;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Persistence.Repositories;

public class CourseRepository(AppDbContext context) : BaseRepository<Course, Guid, AppDbContext>(context), ICourseRepository
{

}


public class CourseMaterialRepository(AppDbContext context) : BaseRepository<CourseMaterial, Guid, AppDbContext>(context), ICourseMaterialRepository
{

}
