namespace EduTrack.Application.DTOs.Course;

/// <summary>A student on a course roster, with assignment progress for the instructor.</summary>
public class CourseStudentDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public int SubmittedCount { get; set; }
    public int AssignmentCount { get; set; }

    /// <summary>Average of graded submissions as a percentage of their points, or null.</summary>
    public double? AveragePercent { get; set; }

    public string Initials =>
        string.Concat(FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));
}
