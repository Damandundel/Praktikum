using System.ComponentModel.DataAnnotations;
using Praktikum.Common;

namespace Praktikum.Web.ViewModels.Account;

/// <summary>Данни от формата за регистрация на ученик.</summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "Името е задължително.")]
    [StringLength(GlobalConstants.Lengths.PersonName, MinimumLength = 2,
        ErrorMessage = "Името трябва да е между {2} и {1} символа.")]
    [Display(Name = "Име")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилията е задължителна.")]
    [StringLength(GlobalConstants.Lengths.PersonName, MinimumLength = 2,
        ErrorMessage = "Фамилията трябва да е между {2} и {1} символа.")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Класът е задължителен.")]
    [StringLength(GlobalConstants.Lengths.ClassName, ErrorMessage = "Класът е твърде дълъг.")]
    [RegularExpression(@"^(?:[1-9]|1[0-2])\s?[А-Яа-яA-Za-z]$",
        ErrorMessage = "Въведете клас във формат като 12А.")]
    [Display(Name = "Клас")]
    public string ClassName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имейлът е задължителен.")]
    [EmailAddress(ErrorMessage = "Въведете валиден имейл адрес.")]
    [StringLength(GlobalConstants.Lengths.Email, ErrorMessage = "Имейлът е твърде дълъг.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Паролата е задължителна.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Паролата трябва да е поне {2} символа.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Потвърдете паролата.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат.")]
    [Display(Name = "Потвърждение на паролата")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
