using EduTrack.Application.DTOs.Course;
using EduTrack.Application.DTOs.CourseMaterial;
using EduTrack.Application.DTOs.StreamPost;

namespace EduTrack.Web.Models.Course;

public class CourseDetailViewModel
{
    public CourseDto CourseDetail { get; set; } = new();
    public List<StreamPostDto> StreamPosts { get; set; } = new();
    public List<CourseMaterialDto> Materials { get; set; } = new();
}
