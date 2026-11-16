using Microsoft.AspNetCore.Identity;

namespace Praktikum.Web.Infrastructure;

/// <summary>
/// Превежда стандартните грешки на Identity на български,
/// за да виждат потребителите ясни съобщения във формите.
/// </summary>
public class BulgarianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
        => new() { Code = nameof(DefaultError), Description = "Възникна неочаквана грешка." };

    public override IdentityError DuplicateEmail(string email)
        => new() { Code = nameof(DuplicateEmail), Description = "Вече има регистриран потребител с този имейл." };

    public override IdentityError DuplicateUserName(string userName)
        => new() { Code = nameof(DuplicateUserName), Description = "Вече има регистриран потребител с този имейл." };

    public override IdentityError InvalidEmail(string? email)
        => new() { Code = nameof(InvalidEmail), Description = "Въведете валиден имейл адрес." };

    public override IdentityError InvalidUserName(string? userName)
        => new() { Code = nameof(InvalidUserName), Description = "Въведете валиден имейл адрес." };

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = $"Паролата трябва да е поне {length} символа." };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = "Паролата трябва да съдържа поне една цифра." };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = "Паролата трябва да съдържа поне една малка буква." };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = "Паролата трябва да съдържа поне една главна буква." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Паролата трябва да съдържа поне един специален символ (например # или !)." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Паролата трябва да съдържа поне {uniqueChars} различни символа." };
}
