namespace Praktikum.Common;

/// <summary>Константи, ползвани във всички слоеве на приложението.</summary>
public static class GlobalConstants
{
    public const string SystemName = "Praktikum";

    /// <summary>Имена на ролите. Ползват се в [Authorize(Roles = ...)] и в seeding.</summary>
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Coordinator = "Coordinator";
        public const string Employer = "Employer";
        public const string Mentor = "Mentor";
        public const string Student = "Student";
    }

    /// <summary>Имена на политиките за оторизация (комбинации от роли).</summary>
    public static class Policies
    {
        public const string CoordinatorOrAdministrator = nameof(CoordinatorOrAdministrator);
    }

    /// <summary>Максимални дължини на текстови полета (общи за модели и валидации).</summary>
    public static class Lengths
    {
        public const int Name = 150;
        public const int Title = 200;
        public const int ShortText = 500;
        public const int LongText = 4000;
        public const int Email = 256;
        public const int Phone = 30;
        public const int PersonName = 60;
        public const int ClassName = 10;
    }

    /// <summary>Ограничения за критериите за оценяване.</summary>
    public static class Grading
    {
        /// <summary>Най-много точки, които може да има един критерий.</summary>
        public const int MaxCriterionScore = 10;
    }

    /// <summary>Бизнес ограничения за дневника.</summary>
    public static class Diary
    {
        public const double MaxHoursPerDay = 12;
    }
}
