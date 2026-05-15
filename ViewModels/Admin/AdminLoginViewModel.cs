using System.ComponentModel.DataAnnotations;

namespace DevLog.ViewModels.Admin;

public class AdminLoginViewModel
{
    [Required]
    [Display(Name = "Логин")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
