using Core.Exceptions;
using Core.Results;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.Assignment;
using EduTrack.Application.DTOs.File;
using EduTrack.Application.DTOs.StreamPost;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Services.Concrete;

public class AssignmentService(
    IAssignmentRepository assignmentRepository,
    IAssignmentSubmissionRepository submissionRepository,
    ICourseRepository courseRepository,
    ICourseStudentRepository courseStudentRepository,
    IStreamPostService streamPostService,
    IFileService fileService,
    AssignmentBusinessRules rules,
    TimeProvider timeProvider) : IAssignmentService
{
    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public async Task<Result<List<AssignmentListItemDto>>> GetCourseAssignmentsAsync(Guid courseId, Guid userId)
    {
        try
        {
            var course = await courseRepository.GetAsync(c => c.Id == courseId, enableTracking: false);
            rules.CheckCourseExists(course);

            var isEnrolled = await IsEnrolledAsync(courseId, userId);
            rules.CheckIsCourseMember(course!, userId, isEnrolled);
            var isInstructor = course!.InstructorId == userId;

            var assignments = await assignmentRepository.GetListAsync(
                predicate: a => a.CourseId == courseId,
                orderBy: q => q.OrderBy(a => a.DueDate),
                include: q => q.Include(a => a.Submissions),
                enableTracking: false);

            var items = assignments.Select(a =>
            {
                var item = new AssignmentListItemDto
                {
                    Id = a.Id,
                    CourseId = a.CourseId,
                    Title = a.Title,
                    DueDate = a.DueDate,
                    MaxPoints = a.MaxPoints,
                    SubmissionCount = a.Submissions.Count,
                    GradedCount = a.Submissions.Count(s => s.Grade is not null),
                };

                if (!isInstructor)
                {
                    var mine = a.Submissions.FirstOrDefault(s => s.StudentId == userId);
                    item.Status = GetStatus(a, mine, UtcNow);
                    item.Grade = mine?.Grade;
                }

                return item;
            }).ToList();

            return Result<List<AssignmentListItemDto>>.Ok(items);
        }
        catch (BusinessException ex)
        {
            return Result<List<AssignmentListItemDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<AssignmentDetailDto>> GetAssignmentDetailAsync(Guid assignmentId, Guid userId)
    {
        try
        {
            var assignment = await assignmentRepository.GetAsync(
                a => a.Id == assignmentId,
                include: q => q.Include(a => a.Course).Include(a => a.Submissions).ThenInclude(s => s.Student),
                enableTracking: false);
            rules.CheckAssignmentExists(assignment);

            var course = assignment!.Course;
            var isEnrolled = await IsEnrolledAsync(course.Id, userId);
            rules.CheckIsCourseMember(course, userId, isEnrolled);

            var isInstructor = course.InstructorId == userId;
            var dto = new AssignmentDetailDto
            {
                Id = assignment.Id,
                CourseId = course.Id,
                CourseTitle = course.Title,
                Title = assignment.Title,
                Instructions = assignment.Instructions,
                DueDate = assignment.DueDate,
                MaxPoints = assignment.MaxPoints,
                CreatedAt = assignment.CreatedAt,
                IsInstructor = isInstructor,
                IsPastDue = assignment.DueDate < UtcNow,
            };

            if (isInstructor)
            {
                var students = await courseStudentRepository.GetListAsync(
                    predicate: cs => cs.CourseId == course.Id,
                    include: q => q.Include(cs => cs.User),
                    enableTracking: false);

                dto.Students = students
                    .Select(cs => cs.User)
                    .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                    .Select(u => new StudentSubmissionRowDto
                    {
                        StudentId = u.Id,
                        StudentName = $"{u.FirstName} {u.LastName}",
                        StudentEmail = u.Email,
                        Submission = ToDto(assignment, assignment.Submissions.FirstOrDefault(s => s.StudentId == u.Id)),
                    })
                    .ToList();
            }
            else
            {
                var mine = assignment.Submissions.FirstOrDefault(s => s.StudentId == userId);
                dto.MySubmission = ToDto(assignment, mine);
                dto.MyStatus = GetStatus(assignment, mine, UtcNow);
            }

            return Result<AssignmentDetailDto>.Ok(dto);
        }
        catch (BusinessException ex)
        {
            return Result<AssignmentDetailDto>.Fail(ex.Message);
        }
    }

    public async Task<Result<Guid>> CreateAssignmentAsync(CreateAssignmentRequest request)
    {
        try
        {
            var course = await courseRepository.GetAsync(c => c.Id == request.CourseId);
            rules.CheckCourseExists(course);
            rules.CheckIsCourseInstructor(course!, request.InstructorId);
            rules.CheckDueDateIsInFuture(request.DueDate, UtcNow);

            var assignment = await assignmentRepository.AddAsync(new Assignment
            {
                CourseId = request.CourseId,
                CreatedById = request.InstructorId,
                Title = request.Title.Trim(),
                Instructions = request.Instructions.Trim(),
                DueDate = request.DueDate,
                MaxPoints = request.MaxPoints,
                CreatedAt = UtcNow,
            });

            // Announce it in the course stream; the post content carries the assignment id.
            await streamPostService.CreateStreamPost(new CreateStreamPostRequest
            {
                CourseId = request.CourseId,
                InstructorId = request.InstructorId,
                Type = StreamPostType.Assignment,
                Content = assignment.Id.ToString(),
            });

            return Result<Guid>.Ok(assignment.Id, "Ödev oluşturuldu.");
        }
        catch (BusinessException ex)
        {
            return Result<Guid>.Fail(ex.Message);
        }
    }

    public async Task<Result> SubmitAsync(SubmitAssignmentRequest request)
    {
        try
        {
            var assignment = await assignmentRepository.GetAsync(a => a.Id == request.AssignmentId, enableTracking: false);
            rules.CheckAssignmentExists(assignment);

            var isEnrolled = await IsEnrolledAsync(assignment!.CourseId, request.StudentId);
            rules.CheckIsEnrolledStudent(isEnrolled);

            var existing = await submissionRepository.GetAsync(
                s => s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId);
            rules.CheckNotGraded(existing);

            string? attachmentUrl = existing?.AttachmentUrl;
            if (request.Attachment is { Length: > 0 })
            {
                await using var stream = request.Attachment.OpenReadStream();
                var upload = await fileService.UploadAsync(new FileUploadRequest(stream, request.Attachment.FileName, request.Attachment.ContentType));
                attachmentUrl = upload.OpenUrl;
            }

            if (existing is null)
            {
                await submissionRepository.AddAsync(new AssignmentSubmission
                {
                    AssignmentId = request.AssignmentId,
                    StudentId = request.StudentId,
                    Text = request.Text.Trim(),
                    AttachmentUrl = attachmentUrl,
                    SubmittedAt = UtcNow,
                });
            }
            else
            {
                existing.Text = request.Text.Trim();
                existing.AttachmentUrl = attachmentUrl;
                existing.SubmittedAt = UtcNow;
                await submissionRepository.UpdateAsync(existing);
            }

            var late = UtcNow > assignment.DueDate;
            return Result.Ok(late ? "Ödeviniz teslim edildi (geç teslim)." : "Ödeviniz teslim edildi.");
        }
        catch (BusinessException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<Guid>> GradeAsync(GradeSubmissionRequest request)
    {
        try
        {
            var submission = await submissionRepository.GetAsync(
                s => s.Id == request.SubmissionId,
                include: q => q.Include(s => s.Assignment).ThenInclude(a => a.Course));
            rules.CheckSubmissionExists(submission);

            var assignment = submission!.Assignment;
            rules.CheckIsCourseInstructor(assignment.Course, request.InstructorId);
            rules.CheckGradeInRange(request.Grade, assignment.MaxPoints);

            submission.Grade = request.Grade;
            submission.Feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : request.Feedback.Trim();
            submission.GradedAt = UtcNow;
            await submissionRepository.UpdateAsync(submission);

            return Result<Guid>.Ok(assignment.Id, "Not kaydedildi.");
        }
        catch (BusinessException ex)
        {
            return Result<Guid>.Fail(ex.Message);
        }
    }

    /// <summary>Status of an assignment for one student, given their submission (if any).</summary>
    public static AssignmentStatus GetStatus(Assignment assignment, AssignmentSubmission? submission, DateTime utcNow)
    {
        if (submission is null)
            return utcNow > assignment.DueDate ? AssignmentStatus.Missing : AssignmentStatus.Open;

        if (submission.Grade is not null)
            return AssignmentStatus.Graded;

        return submission.SubmittedAt > assignment.DueDate ? AssignmentStatus.SubmittedLate : AssignmentStatus.Submitted;
    }

    private Task<bool> IsEnrolledAsync(Guid courseId, Guid userId) =>
        courseStudentRepository.AnyAsync(cs => cs.CourseId == courseId && cs.UserId == userId);

    private static SubmissionDto? ToDto(Assignment assignment, AssignmentSubmission? submission) =>
        submission is null
            ? null
            : new SubmissionDto
            {
                Id = submission.Id,
                Text = submission.Text,
                AttachmentUrl = submission.AttachmentUrl,
                SubmittedAt = submission.SubmittedAt,
                IsLate = submission.SubmittedAt > assignment.DueDate,
                Grade = submission.Grade,
                Feedback = submission.Feedback,
                GradedAt = submission.GradedAt,
            };
}
