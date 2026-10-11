using Core.BusinessRules;
using Core.Exceptions;
using Core.Security;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;


namespace EduTrack.Application.BusinessRules;

public class StreamPostBusinessRules : IBusinessRule
{
    public void CheckCourseExists(Course? course)
    {
        if (course is null)
           throw new BusinessException("Kurs bulunamadı.");
    }
}
