using EduTrack.Application.DTOs.Assignment;
using EduTrack.Application.Services.Concrete;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.UnitTest.Services;

public class AssignmentStatusTests
{
    private static readonly DateTime Due = new(2026, 3, 10, 20, 59, 0, DateTimeKind.Utc);
    private static readonly Assignment Assignment = new() { Title = "Ödev", DueDate = Due, MaxPoints = 100 };

    [Fact]
    public void No_submission_before_due_date_is_open()
    {
        Assert.Equal(AssignmentStatus.Open, AssignmentService.GetStatus(Assignment, null, Due.AddHours(-1)));
    }

    [Fact]
    public void No_submission_after_due_date_is_missing()
    {
        Assert.Equal(AssignmentStatus.Missing, AssignmentService.GetStatus(Assignment, null, Due.AddMinutes(1)));
    }

    [Fact]
    public void Submission_on_time_is_submitted()
    {
        var submission = new AssignmentSubmission { SubmittedAt = Due.AddDays(-1) };

        Assert.Equal(AssignmentStatus.Submitted, AssignmentService.GetStatus(Assignment, submission, Due.AddDays(2)));
    }

    [Fact]
    public void Submission_after_due_date_is_late()
    {
        var submission = new AssignmentSubmission { SubmittedAt = Due.AddHours(3) };

        Assert.Equal(AssignmentStatus.SubmittedLate, AssignmentService.GetStatus(Assignment, submission, Due.AddDays(1)));
    }

    [Fact]
    public void Graded_submission_is_graded_even_when_late()
    {
        var submission = new AssignmentSubmission { SubmittedAt = Due.AddHours(3), Grade = 70 };

        Assert.Equal(AssignmentStatus.Graded, AssignmentService.GetStatus(Assignment, submission, Due.AddDays(1)));
    }
}
