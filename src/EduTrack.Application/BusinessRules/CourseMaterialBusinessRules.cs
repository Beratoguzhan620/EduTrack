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
    public void CheckCourseExists(Course? course)
    {
        if (course is null)
            throw new BusinessException("Kurs bulunamadı.");
    }

    public void CheckCourseMaterialExists(CourseMaterial? courseMaterial)
    {
        if (courseMaterial is null)
            throw new BusinessException("Kurs materyali bulunamadı.");
    }

    public void DescriptionShouldNotBeEmptyWhenTypeIsContent(CreateCourseMaterialRequest request)
    {
        if (request.Type == MaterialType.Content && string.IsNullOrWhiteSpace(request.Description))
            throw new BusinessException("İçerik tipli materyalin açıklaması boş olamaz.");
    }

    public void UrlShouldNotBeEmptyWhenTypeIsLink(CreateCourseMaterialRequest request)
    {
        if (request.Type == MaterialType.Link && string.IsNullOrWhiteSpace(request.Url))
            throw new BusinessException("Link tipli materyalin URL'si boş olamaz.");
    }

    public void FileShouldNotBeNullWhenTypeIsDocument(CreateCourseMaterialRequest request)
    {
        if (request.Type == MaterialType.Document && request.File is null)
            throw new BusinessException("Dosya tipli materyalin dosya içeriği boş olamaz.");
    }
}
