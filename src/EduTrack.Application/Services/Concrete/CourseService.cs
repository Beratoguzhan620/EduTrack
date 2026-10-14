using Core.Exceptions;
using Core.Results;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.Course;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Services.Concrete;

public class CourseService(
    IUserRepository userRepository, 
    ICourseRepository courseRepository,
    ICourseStudentRepository courseStudentRepository,
    IAssignmentRepository assignmentRepository,
    CourseBusinessRules rules,
    TimeProvider timeProvider) : ICourseService
{

    public async Task<Result<List<CourseDto>>> GetUserCoursesAsync(Guid userId)
    {
        var courses = new List<CourseDto>();

        var instructorCourses = await  courseRepository.GetListAsync(c => c.InstructorId == userId, include: c => c.Include(c => c.Instructor).Include(c => c.Students));
        var studentCourses = await courseRepository.GetListAsync(c => c.Students.Any(s => s.UserId == userId), include: c => c.Include(c => c.Instructor).Include(c => c.Students));

        courses.AddRange(instructorCourses.Select(c => new CourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            InstructorFullName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
            IsInstructor = true,
            StudentCount = c.Students.Count,
            Code = c.Code
        }));

        courses.AddRange(studentCourses.Select(c => new CourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            InstructorFullName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
            IsInstructor = false,
            StudentCount = c.Students.Count,
            Code = c.Code
        }));

        return Result<List<CourseDto>>.Ok(courses);
    }


    public async Task<Result<CourseCreatedResponse>> CreateCourseAsync(CreateCourseRequest request)
    {


        try
        {
            var instructor = await userRepository.GetAsync(u => u.Id == request.InstructorId);

            rules.CheckUserExists(instructor);

            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                InstructorId = request.InstructorId,
                IsActive = true,
                CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
                Code = await GenerateUniqueCourseCode()
            };


            await courseRepository.AddAsync(course);

            var response = new CourseCreatedResponse
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                IsActive = course.IsActive,
                Code = course.Code,
                InstructorId = course.InstructorId,
                CreatedAt = course.CreatedAt
            };

            return Result<CourseCreatedResponse>.Ok(response);

        }
        catch (BusinessException ex)
        {
            return Result<CourseCreatedResponse>.Fail(ex.Message);
        }
    }




    public async Task<Result> JoinCourseAsync(JoinCourseRequest request)
    {

        try
        {
            var student = await userRepository.GetAsync(u => u.Id == request.StudentId);
            rules.CheckUserExists(student);

            var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant();
            var course = await courseRepository.GetAsync(c => c.Code == code);
            rules.CheckCourseCodeExists(course);
            rules.CheckIsNotOwnCourse(course!, student!.Id);

            await rules.CheckStudentAlreadyEnrolled(course!, student);

            var courseStudent = new CourseStudent
            {
                CourseId = course!.Id,
                UserId = student!.Id,
                EnrolledAt = timeProvider.GetUtcNow().UtcDateTime
            };

            await courseStudentRepository.AddAsync(courseStudent);

            return Result.Ok($"{course!.Title} dersine katıldınız.");
        }
        catch (BusinessException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<CourseDto>> GetCourseDetailAsync(Guid courseId, Guid userId)
    {
        try
        {
            var course = await courseRepository.GetAsync(
                c => c.Id == courseId,
                include: c => c.Include(c => c.Instructor).Include(c => c.Students));

            rules.CheckCourseExists(course);
            rules.CheckIsCourseMember(course!, userId, course!.Students.Any(st => st.UserId == userId));

            var courseDto = new CourseDto
            {
                Id = course!.Id,
                Title = course.Title,
                Description = course.Description,
                InstructorFullName = $"{course.Instructor.FirstName} {course.Instructor.LastName}",
                IsInstructor = course.InstructorId == userId,
                StudentCount = course.Students.Count,
                Code = course.Code
            };

            return Result<CourseDto>.Ok(courseDto);
        }
        catch (BusinessException ex)
        {
            return Result<CourseDto>.Fail(ex.Message);
        }

    }


    public async Task<Result<List<CourseStudentDto>>> GetCourseStudentsAsync(Guid courseId, Guid userId)
    {
        try
        {
            var course = await courseRepository.GetAsync(
                c => c.Id == courseId,
                include: c => c.Include(c => c.Students).ThenInclude(cs => cs.User),
                enableTracking: false);

            rules.CheckCourseExists(course);
            rules.CheckIsCourseMember(course!, userId, course!.Students.Any(st => st.UserId == userId));

            var assignments = await assignmentRepository.GetListAsync(
                predicate: a => a.CourseId == courseId,
                include: q => q.Include(a => a.Submissions),
                enableTracking: false);

            var students = course.Students
                .OrderBy(cs => cs.User.FirstName).ThenBy(cs => cs.User.LastName)
                .Select(cs =>
                {
                    var submissions = assignments
                        .SelectMany(a => a.Submissions.Where(s => s.StudentId == cs.UserId).Select(s => (Assignment: a, Submission: s)))
                        .ToList();
                    var graded = submissions.Where(x => x.Submission.Grade is not null && x.Assignment.MaxPoints > 0).ToList();

                    return new CourseStudentDto
                    {
                        UserId = cs.UserId,
                        FullName = $"{cs.User.FirstName} {cs.User.LastName}",
                        Email = cs.User.Email,
                        EnrolledAt = cs.EnrolledAt,
                        AssignmentCount = assignments.Count,
                        SubmittedCount = submissions.Count,
                        AveragePercent = graded.Count == 0
                            ? null
                            : graded.Average(x => x.Submission.Grade!.Value * 100.0 / x.Assignment.MaxPoints),
                    };
                })
                .ToList();

            return Result<List<CourseStudentDto>>.Ok(students);
        }
        catch (BusinessException ex)
        {
            return Result<List<CourseStudentDto>>.Fail(ex.Message);
        }
    }

    private async Task<string> GenerateUniqueCourseCode()
    {
        string code;
        do
        {
            code = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        } while (await courseRepository.AnyAsync(c => c.Code == code));
        return code;
    }


}
