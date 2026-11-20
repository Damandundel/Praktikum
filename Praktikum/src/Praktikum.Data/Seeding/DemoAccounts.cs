using Praktikum.Common;

namespace Praktikum.Data.Seeding;

/// <summary>
/// Демо акаунти за показване на системата. Паролата е ПУБЛИЧНА и умишлено документирана в README;
/// тя не пази нищо реално и не бива да се ползва никъде другаде.
/// </summary>
public static class DemoAccounts
{
    public const string Password = "Demo#2027pass";

    public const string AdministratorEmail = "admin@praktikum.demo";
    public const string CoordinatorEmail = "coordinator@praktikum.demo";
    public const string EmployerEmail = "employer@praktikum.demo";
    public const string MentorEmail = "mentor@praktikum.demo";
    public const string StudentEmail = "student@praktikum.demo";

    /// <summary>Всички демо акаунти: имейл, роля, име, фамилия.</summary>
    public static readonly (string Email, string Role, string FirstName, string LastName)[] All =
    {
        (AdministratorEmail, GlobalConstants.Roles.Administrator, "Демо", "Администратор"),
        (CoordinatorEmail, GlobalConstants.Roles.Coordinator, "Демо", "Координатор"),
        (EmployerEmail, GlobalConstants.Roles.Employer, "Демо", "Работодател"),
        (MentorEmail, GlobalConstants.Roles.Mentor, "Демо", "Наставник"),
        (StudentEmail, GlobalConstants.Roles.Student, "Демо", "Ученик"),
    };
}
