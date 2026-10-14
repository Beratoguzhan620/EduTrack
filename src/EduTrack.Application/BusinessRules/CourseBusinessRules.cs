using System.Diagnostics.CodeAnalysis;
using Core.BusinessRules;
using Core.Exceptions;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.BusinessRules;

public class CourseBusinessRules(ICourseStudentRepository courseStudentRepository) : IBusinessRule
{
    public void CheckUserExists([NotNull] User? user)
    {
        if (user is null)
            throw new BusinessException("Kullanıcı bulunamadı.");
    }

    public void CheckCourseExists([NotNull] Course? course)
    {
        if (course is null)
            throw new BusinessException("Ders bulunamadı.");
    }

    public void CheckCourseCodeExists([NotNull] Course? course)
    {
        if (course is null)
            throw new BusinessException("Bu koda sahip bir ders bulunamadı. Kodu kontrol edin.");
    }

    public void CheckIsNotOwnCourse(Course course, Guid userId)
    {
        if (course.InstructorId == userId)
            throw new BusinessException("Kendi verdiğiniz derse öğrenci olarak katılamazsınız.");
    }

    public void CheckIsCourseMember(Course course, Guid userId, bool isEnrolled)
    {
        if (course.InstructorId != userId && !isEnrolled)
            throw new BusinessException("Bu derse kayıtlı değilsiniz.");
    }

    public async Task CheckStudentAlreadyEnrolled(Course course, User student)
    {
        if (await courseStudentRepository.AnyAsync(cs => cs.CourseId == course.Id && cs.UserId == student.Id))
            throw new BusinessException("Bu derse zaten kayıtlısınız.");
    }
}
