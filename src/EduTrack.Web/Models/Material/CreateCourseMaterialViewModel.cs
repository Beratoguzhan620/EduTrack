using System.ComponentModel.DataAnnotations;
using EduTrack.Domain.Enums;

namespace EduTrack.Web.Models.Material;

public class CreateCourseMaterialViewModel
{
    public Guid CourseId { get; set; }

    [Display(Name = "Başlık")]
    [Required(ErrorMessage = "Başlık gereklidir."), MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "İçerik")]
    [MaxLength(4000, ErrorMessage = "İçerik en fazla 4000 karakter olabilir.")]
    public string? Description { get; set; }

    [Display(Name = "Tür")]
    [Required(ErrorMessage = "Materyal türünü seçin.")]
    public MaterialType? Type { get; set; }

    [Display(Name = "Bağlantı")]
    public string? Url { get; set; }

    [Display(Name = "Dosya")]
    public IFormFile? File { get; set; }

    // Display only.
    public string CourseTitle { get; set; } = string.Empty;
}
