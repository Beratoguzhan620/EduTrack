using System.Diagnostics.CodeAnalysis;
using Core.BusinessRules;
using Core.Exceptions;
using EduTrack.Application.DTOs.CourseMaterial;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.BusinessRules;

public class CourseMaterialBusinessRules : IBusinessRule
{
    public void CheckCourseExists([NotNull] Course? course)
    {
        if (course is null)
            throw new BusinessException("Ders bulunamadı.");
    }

    public void CheckIsCourseInstructor(Course course, Guid userId)
    {
        if (course.InstructorId != userId)
            throw new BusinessException("Materyal yalnızca dersin eğitmeni tarafından eklenebilir.");
    }

    public void CheckIsCourseMember(Course course, Guid userId, bool isEnrolled)
    {
        if (course.InstructorId != userId && !isEnrolled)
            throw new BusinessException("Bu derse kayıtlı değilsiniz.");
    }

    public void CheckLinkIsWebAddress(CreateCourseMaterialRequest request)
    {
        if (request.Type == MaterialType.Link
            && (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")))
            throw new BusinessException("Bağlantı http:// veya https:// ile başlayan geçerli bir adres olmalıdır.");
    }

    public void CheckCourseMaterialExists([NotNull] CourseMaterial? courseMaterial)
    {
        if (courseMaterial is null)
            throw new BusinessException("Materyal bulunamadı.");
    }

    public void DescriptionShouldNotBeEmptyWhenTypeIsContent(CreateCourseMaterialRequest request)
    {
        if (request.Type == MaterialType.Content && string.IsNullOrWhiteSpace(request.Description))
            throw new BusinessException("Metin materyalinin içeriği boş olamaz.");
    }

    public void UrlShouldNotBeEmptyWhenTypeIsLink(CreateCourseMaterialRequest request)
    {
        if (request.Type == MaterialType.Link && string.IsNullOrWhiteSpace(request.Url))
            throw new BusinessException("Bağlantı materyali için adres girin.");
    }

    public void FileShouldNotBeNullWhenTypeIsDocument(CreateCourseMaterialRequest request)
    {
        if (request.Type == MaterialType.Document && request.File is null)
            throw new BusinessException("Doküman materyali için bir dosya seçin.");
    }
}
