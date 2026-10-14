using System.Diagnostics.CodeAnalysis;
using Core.BusinessRules;
using Core.Exceptions;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.BusinessRules;

public class AssignmentBusinessRules : IBusinessRule
{
    public void CheckCourseExists([NotNull] Course? course)
    {
        if (course is null)
            throw new BusinessException("Ders bulunamadı.");
    }

    public void CheckAssignmentExists([NotNull] Assignment? assignment)
    {
        if (assignment is null)
            throw new BusinessException("Ödev bulunamadı.");
    }

    public void CheckSubmissionExists([NotNull] AssignmentSubmission? submission)
    {
        if (submission is null)
            throw new BusinessException("Teslim bulunamadı.");
    }

    public void CheckIsCourseInstructor(Course course, Guid userId)
    {
        if (course.InstructorId != userId)
            throw new BusinessException("Bu işlem yalnızca dersin eğitmeni tarafından yapılabilir.");
    }

    public void CheckIsCourseMember(Course course, Guid userId, bool isEnrolled)
    {
        if (course.InstructorId != userId && !isEnrolled)
            throw new BusinessException("Bu derse kayıtlı değilsiniz.");
    }

    public void CheckIsEnrolledStudent(bool isEnrolled)
    {
        if (!isEnrolled)
            throw new BusinessException("Ödev teslim etmek için derse kayıtlı öğrenci olmalısınız.");
    }

    public void CheckDueDateIsInFuture(DateTime dueDateUtc, DateTime utcNow)
    {
        if (dueDateUtc <= utcNow)
            throw new BusinessException("Teslim tarihi ileri bir tarih olmalıdır.");
    }

    public void CheckNotGraded(AssignmentSubmission? submission)
    {
        if (submission?.Grade is not null)
            throw new BusinessException("Notlandırılmış bir teslim değiştirilemez.");
    }

    public void CheckGradeInRange(int grade, int maxPoints)
    {
        if (grade < 0 || grade > maxPoints)
            throw new BusinessException($"Not 0 ile {maxPoints} arasında olmalıdır.");
    }
}
