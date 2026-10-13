using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduTrack.Application.DTOs.Assignment;

public enum AssignmentStatus
{
    /// <summary>Student has not submitted and the due date has not passed.</summary>
    Open,
    /// <summary>Student submitted on time and is waiting for a grade.</summary>
    Submitted,
    /// <summary>Student submitted after the due date.</summary>
    SubmittedLate,
    /// <summary>Student did not submit and the due date has passed.</summary>
    Missing,
    Graded,
}

public class AssignmentListItemDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int MaxPoints { get; set; }

    /// <summary>Status from the current student's point of view (null for instructors).</summary>
    public AssignmentStatus? Status { get; set; }
    public int? Grade { get; set; }

    /// <summary>Instructor view: how many students submitted / how many were graded.</summary>
    public int SubmissionCount { get; set; }
    public int GradedCount { get; set; }
}

public class AssignmentDetailDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int MaxPoints { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsInstructor { get; set; }
    public bool IsPastDue { get; set; }

    /// <summary>The current student's submission (student view only).</summary>
    public SubmissionDto? MySubmission { get; set; }
    public AssignmentStatus? MyStatus { get; set; }

    /// <summary>Every enrolled student with their submission, if any (instructor view only).</summary>
    public List<StudentSubmissionRowDto> Students { get; set; } = [];
}

public class SubmissionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsLate { get; set; }
    public int? Grade { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
}

public class StudentSubmissionRowDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public SubmissionDto? Submission { get; set; }
}

public class CreateAssignmentRequest
{
    public Guid CourseId { get; set; }
    public Guid InstructorId { get; set; }

    [Required(ErrorMessage = "Başlık gereklidir."), MaxLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama gereklidir."), MaxLength(4000, ErrorMessage = "Açıklama en fazla 4000 karakter olabilir.")]
    public string Instructions { get; set; } = string.Empty;

    [Required(ErrorMessage = "Teslim tarihi gereklidir.")]
    public DateTime DueDate { get; set; }

    [Range(1, 1000, ErrorMessage = "Puan 1 ile 1000 arasında olmalıdır.")]
    public int MaxPoints { get; set; } = 100;
}

public class SubmitAssignmentRequest
{
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Cevap metni gereklidir."), MaxLength(8000, ErrorMessage = "Cevap en fazla 8000 karakter olabilir.")]
    public string Text { get; set; } = string.Empty;

    public IFormFile? Attachment { get; set; }
}

public class GradeSubmissionRequest
{
    public Guid SubmissionId { get; set; }
    public Guid InstructorId { get; set; }

    [Range(0, 1000, ErrorMessage = "Not 0 veya daha büyük olmalıdır.")]
    public int Grade { get; set; }

    [MaxLength(2000, ErrorMessage = "Geri bildirim en fazla 2000 karakter olabilir.")]
    public string? Feedback { get; set; }
}
