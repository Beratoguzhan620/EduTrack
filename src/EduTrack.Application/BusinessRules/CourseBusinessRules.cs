using Core.BusinessRules;
using Core.Exceptions;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace EduTrack.Application.BusinessRules;

public class CourseBusinessRules(ICourseStudentRepository courseStudentRepository) : IBusinessRule
{
    public void CheckUserExists(User? user)
    {
        if (user is null)
            throw new BusinessException("Kullanıcı bulunamadı.");
    }

    public void CheckCourseExists(Course? course)
    {
        if (course is null)
            throw new BusinessException("Kurs bulunamadı.");
    }
    public async Task CheckStudentAlreadyEnrolled(Course? course, User? student)
    {
        if(await courseStudentRepository.AnyAsync(cs => cs.CourseId == course.Id && cs.UserId == student.Id))
            throw new BusinessException("Öğrenci zaten bu kursa kayıtlı.");
    }
}
  
