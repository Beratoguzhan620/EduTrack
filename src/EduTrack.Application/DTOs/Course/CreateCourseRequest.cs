using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.DTOs.Course;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
}

