namespace Praktikum.Common;

/// <summary>
/// Нарушено бизнес правило (например "позицията е пълна"). Съобщението е на български
/// и е предназначено да се покаже директно на потребителя.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
