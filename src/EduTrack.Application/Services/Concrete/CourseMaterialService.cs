using Core.Exceptions;
using Core.Results;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.CourseMaterial;
using EduTrack.Application.DTOs.File;
using EduTrack.Application.DTOs.StreamPost;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EduTrack.Application.Services.Concrete;

public class CourseMaterialService(
    ICourseMaterialRepository courseMaterialRepository,
    ICourseRepository courseRepository,
    CourseMaterialBusinessRules rules,
    IFileService fileService,
    IStreamPostService streamPostService
    ) : ICourseMaterialService
{
    public async Task<Result<CourseMaterialDto>> AddCourseMaterialAsync(CreateCourseMaterialRequest request)
    {
        try
        {
            rules.DescriptionShouldNotBeEmptyWhenTypeIsContent(request);
            rules.UrlShouldNotBeEmptyWhenTypeIsLink(request);
            rules.FileShouldNotBeNullWhenTypeIsDocument(request);

            var course = await courseRepository.GetAsync(c => c.Id == request.CourseId);
            rules.CheckCourseExists(course);


            var courseMaterial = new CourseMaterial
            {
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                Url = request.Url,
                CourseId = request.CourseId,
                Date = DateTime.UtcNow
            };

            if (request.Type == MaterialType.Document && request.File is not null)
                courseMaterial.Url = await UploadFileAsync(request.File);

            var entity = await courseMaterialRepository.AddAsync(courseMaterial);


            var streamPostRequest = new CreateStreamPostRequest
            {
                Content = entity.Id.ToString(),
                Type = StreamPostType.Material,
                CourseId = entity.CourseId,
                InstructorId = request.InstructorId
            };

            await streamPostService.CreateStreamPost(streamPostRequest);

            return Result<CourseMaterialDto>.Ok(new CourseMaterialDto
            {
                Id = courseMaterial.Id,
                Title = courseMaterial.Title,
                Description = courseMaterial.Description,
                Type = courseMaterial.Type,
                Url = courseMaterial.Url,
                CourseId = courseMaterial.CourseId
            });


        }
        catch (BusinessException e)
        {

            return Result<CourseMaterialDto>.Fail(e.Message);
        }
        catch (Exception e)
        {
            return Result<CourseMaterialDto>.Fail(e.Message);

        }
    }

    private async Task<string> UploadFileAsync(IFormFile file)
    {
        var uploadRequest = new FileUploadRequest(file.OpenReadStream(), file.FileName, file.ContentType);
        var uploadResult = await fileService.UploadAsync(uploadRequest);
        return uploadResult.OpenUrl;
    }

    public async Task<Result<CourseMaterialDto>> GetCourseMaterialByIdAsync(Guid courseMaterialId)
    {
        try
        {
            var material = await courseMaterialRepository.GetAsync(cm => cm.Id == courseMaterialId);
            rules.CheckCourseMaterialExists(material);
            var dto = new CourseMaterialDto
            {
                Id = material.Id,
                Title = material.Title,
                Description = material.Description,
                Type = material.Type,
                Url = material.Url,
                CourseId = material.CourseId,
                Date = material.Date
                
            };
            return Result<CourseMaterialDto>.Ok(dto);

        }
        catch (BusinessException e)
        {

            return Result<CourseMaterialDto>.Fail(e.Message);
        }
    }

    public async Task<Result<List<CourseMaterialDto>>> GetCourseMaterialsByCourseIdAsync(Guid courseId)
    {
        try
        {
            var course = await courseRepository.GetAsync(c => c.Id == courseId);
            rules.CheckCourseExists(course);

            var materials = await courseMaterialRepository.GetListAsync(
                predicate: a => a.CourseId == courseId,
                orderBy: q => q.OrderByDescending(a => a.Date));

            var dto = materials.Select(cm => new CourseMaterialDto
            {
                Id = cm.Id,
                Title = cm.Title,
                Description = cm.Description,
                Type = cm.Type,
                Url = cm.Url,
                CourseId = cm.CourseId,
                Date = cm.Date
            }).ToList();

            return Result<List<CourseMaterialDto>>.Ok(dto);
        }
        catch (BusinessException e)
        {
            return Result<List<CourseMaterialDto>>.Fail(e.Message);
        }
    }

}
