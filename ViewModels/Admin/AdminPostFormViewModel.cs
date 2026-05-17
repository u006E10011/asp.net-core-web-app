using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DevLog.ViewModels.Admin;

public class AdminPostFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(160)]
    [Display(Name = "Название")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(10000)]
    [Display(Name = "Текст")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Хештеги")]
    public string TagsInput { get; set; } = string.Empty;

    [Display(Name = "Изображение")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "Лайки")]
    [Range(0, int.MaxValue)]
    public int LikesCount { get; set; }

    [Display(Name = "Дизлайки")]
    [Range(0, int.MaxValue)]
    public int DislikesCount { get; set; }

    public bool RemoveImage { get; set; }

    public bool HasExistingImage { get; set; }
}
