using System.Linq.Expressions;
using Core.Exceptions;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace EduTrack.Application.UnitTest.BusinessRules;

public class CourseBusinessRulesTests
{
    private readonly Mock<ICourseStudentRepository> _enrollments = new();
    private readonly CourseBusinessRules _rules;
    private static readonly Guid InstructorId = Guid.NewGuid();
    private static readonly Course Course = new() { Id = Guid.NewGuid(), Title = "5-A Matematik", InstructorId = InstructorId };

    public CourseBusinessRulesTests() => _rules = new CourseBusinessRules(_enrollments.Object);

    private void EnrollmentExists(bool exists) =>
        _enrollments
            .Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<CourseStudent, bool>>?>(),
                It.IsAny<Func<IQueryable<CourseStudent>, IIncludableQueryable<CourseStudent, object>>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

    [Fact]
    public void Missing_course_or_user_is_reported()
    {
        Assert.Throws<BusinessException>(() => _rules.CheckCourseExists(null));
        Assert.Throws<BusinessException>(() => _rules.CheckUserExists(null));

        var ex = Assert.Throws<BusinessException>(() => _rules.CheckCourseCodeExists(null));
        Assert.Contains("kod", ex.Message);
    }

    [Fact]
    public void Instructor_cannot_join_own_course()
    {
        Assert.Throws<BusinessException>(() => _rules.CheckIsNotOwnCourse(Course, InstructorId));
        _rules.CheckIsNotOwnCourse(Course, Guid.NewGuid());
    }

    [Fact]
    public void Only_members_can_open_a_course()
    {
        _rules.CheckIsCourseMember(Course, InstructorId, isEnrolled: false);
        _rules.CheckIsCourseMember(Course, Guid.NewGuid(), isEnrolled: true);

        Assert.Throws<BusinessException>(() => _rules.CheckIsCourseMember(Course, Guid.NewGuid(), isEnrolled: false));
    }

    [Fact]
    public async Task Enrolling_twice_is_rejected()
    {
        var student = new User { Id = Guid.NewGuid() };

        EnrollmentExists(true);
        await Assert.ThrowsAsync<BusinessException>(() => _rules.CheckStudentAlreadyEnrolled(Course, student));

        EnrollmentExists(false);
        await _rules.CheckStudentAlreadyEnrolled(Course, student);
    }
}
