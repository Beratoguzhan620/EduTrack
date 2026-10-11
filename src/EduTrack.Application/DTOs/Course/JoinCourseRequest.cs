using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.DTOs.Course;

public class JoinCourseRequest
{
    public Guid StudentId { get; set; }
    public string Code { get; set; } = string.Empty;
} 
