using Core.Exceptions;
using Core.Results;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.StreamPost;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EduTrack.Application.Services.Concrete;

public class StreamPostService(
    IStreamPostRepository streamPostRepository,
    ICourseRepository courseRepository,
    StreamPostBusinessRules rules) : IStreamPostService
{
    public async Task<Result<List<StreamPostDto>>> GetStreamPosts(Guid courseId)
    {
        var streamPosts = await streamPostRepository.GetListAsync(
            predicate: sp => sp.CourseId == courseId, 
            orderBy: sp => sp.OrderByDescending(s => s.Date),
            include: sp => sp.Include(sp => sp.Course).ThenInclude(c => c.Instructor));

        var result = streamPosts.Select(sp => new StreamPostDto
        {
            CourseId = sp.CourseId,
            Title = sp.Title,
            InstructorInitials = GetInstructorInitials(sp.Course.Instructor),
            Content = sp.Content,
            Type = sp.Type,
            Date = sp.Date
        }).ToList();

        return Result<List<StreamPostDto>>.Ok(result);
    }

    public async Task<Result> CreateStreamPost(CreateStreamPostRequest request)
    {
        try
        {
            var course = await courseRepository.GetAsync(
                predicate: c => c.Id == request.CourseId && c.InstructorId == request.InstructorId,
                include: c => c.Include(c => c.Instructor));

            rules.CheckCourseExists(course);

            string title = GenerateStreamPostTitle(request.Type, course!.Instructor);

            var streamPost = new StreamPost
            {
                CourseId = request.CourseId,
                Content = request.Content,
                Title = title,
                Type = request.Type,
                InstructorId = request.InstructorId,
                Date = DateTime.UtcNow
            };

            await streamPostRepository.AddAsync(streamPost);
            return Result.Ok();
        }
        catch (BusinessException e)
        {
            return Result.Fail(e.Message);
        }

    }

    private static string GenerateStreamPostTitle(StreamPostType type, User instructor)
    {
        var instructorName = $"{instructor.FirstName} {instructor.LastName}";

        return type switch
        {
            StreamPostType.Announcement => $"{instructorName}",
            StreamPostType.Assignment => $"{instructorName} posted a new assignment",
            StreamPostType.Material => $"{instructorName} shared material",
            _ => "Stream Post"
        };
    }


    private static string GetInstructorInitials(User instructor)
    {
        var instructorName = $"{instructor.FirstName} {instructor.LastName}";

        var initials = string.Join("", instructorName.Split(' ').Select(n => n[0])).ToUpper();
        return initials;
    }


}
