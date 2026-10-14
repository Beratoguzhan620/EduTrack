using System.ComponentModel.DataAnnotations;

namespace EduTrack.Web.Models.Course;

public class CreateCourseViewModel
{
    [Display(Name = "Ders adı")]
    [Required(ErrorMessage = "Ders adı gereklidir."), MaxLength(100, ErrorMessage = "Ders adı en fazla 100 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [Required(ErrorMessage = "Açıklama gereklidir."), MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;
}
