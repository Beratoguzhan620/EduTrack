using System.Diagnostics.CodeAnalysis;
using Core.BusinessRules;
using Core.Exceptions;
using Core.Security;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;


namespace EduTrack.Application.BusinessRules;

public class StreamPostBusinessRules : IBusinessRule
{
    public void CheckCourseExists([NotNull] Course? course)
    {
        if (course is null)
           throw new BusinessException("Ders bulunamadı veya bu derste paylaşım yetkiniz yok.");
    }
}
