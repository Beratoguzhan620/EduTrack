using Core.Exceptions;
using EduTrack.Application.BusinessRules;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.UnitTest.BusinessRules;

public class AssignmentBusinessRulesTests
{
    private readonly AssignmentBusinessRules _rules = new();
    private static readonly Guid InstructorId = Guid.NewGuid();
    private static readonly Course Course = new() { Title = "5-A Matematik", InstructorId = InstructorId };

    [Fact]
    public void Instructor_check_rejects_other_users()
    {
        _rules.CheckIsCourseInstructor(Course, InstructorId);

        Assert.Throws<BusinessException>(() => _rules.CheckIsCourseInstructor(Course, Guid.NewGuid()));
    }

    [Fact]
    public void Course_member_is_instructor_or_enrolled_student()
    {
        _rules.CheckIsCourseMember(Course, InstructorId, isEnrolled: false);
        _rules.CheckIsCourseMember(Course, Guid.NewGuid(), isEnrolled: true);

        Assert.Throws<BusinessException>(() => _rules.CheckIsCourseMember(Course, Guid.NewGuid(), isEnrolled: false));
    }

    [Fact]
    public void Due_date_must_be_in_the_future()
    {
        var now = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc);

        _rules.CheckDueDateIsInFuture(now.AddMinutes(1), now);
        Assert.Throws<BusinessException>(() => _rules.CheckDueDateIsInFuture(now, now));
        Assert.Throws<BusinessException>(() => _rules.CheckDueDateIsInFuture(now.AddDays(-1), now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Grade_within_range_is_accepted(int grade)
    {
        _rules.CheckGradeInRange(grade, 100);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Grade_outside_range_is_rejected(int grade)
    {
        var ex = Assert.Throws<BusinessException>(() => _rules.CheckGradeInRange(grade, 100));
        Assert.Contains("100", ex.Message);
    }

    [Fact]
    public void Graded_submission_cannot_be_changed()
    {
        _rules.CheckNotGraded(null);
        _rules.CheckNotGraded(new AssignmentSubmission());

        Assert.Throws<BusinessException>(() => _rules.CheckNotGraded(new AssignmentSubmission { Grade = 80 }));
    }

    [Fact]
    public void Missing_entities_are_reported()
    {
        Assert.Throws<BusinessException>(() => _rules.CheckCourseExists(null));
        Assert.Throws<BusinessException>(() => _rules.CheckAssignmentExists(null));
        Assert.Throws<BusinessException>(() => _rules.CheckSubmissionExists(null));
        Assert.Throws<BusinessException>(() => _rules.CheckIsEnrolledStudent(false));
    }
}
