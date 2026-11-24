using System.ComponentModel.DataAnnotations;
using Praktikum.Common;

namespace Praktikum.Web.ViewModels.Account;

/// <summary>Данни от формата за вход.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Имейлът е задължителен.")]
    [EmailAddress(ErrorMessage = "Въведете валиден имейл адрес.")]
    [StringLength(GlobalConstants.Lengths.Email, ErrorMessage = "Имейлът е твърде дълъг.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Паролата е задължителна.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомни ме")]
    public bool RememberMe { get; set; }

    /// <summary>Къде да бъде върнат потребителят след входа (проверява се дали е локален адрес).</summary>
    public string? ReturnUrl { get; set; }
}
