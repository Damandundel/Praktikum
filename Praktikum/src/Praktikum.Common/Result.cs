namespace Praktikum.Common;

/// <summary>
/// Неуспех на операция (например "позицията е пълна"). Съобщението е на български и е предназначено
/// да се покаже директно на потребителя. Връща се вместо изключение, защото нарушено бизнес правило
/// е очакван, а не изключителен случай.
/// </summary>
public readonly record struct Failure(string Message);

/// <summary>Резултат от операция без върната стойност: успех или неуспех със съобщение.</summary>
public class Result
{
    private static readonly Result SuccessResult = new(true, string.Empty);

    protected Result(bool succeeded, string error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }

    public bool Failed => !Succeeded;

    /// <summary>Съобщение за грешка; празно при успех.</summary>
    public string Error { get; }

    /// <summary>Неуспехът като <see cref="Failure"/>, за да се върне от метод с друг тип на резултата.</summary>
    public Failure Failure => new(Error);

    public static Result Ok() => SuccessResult;

    public static Result<T> Ok<T>(T value) => new(value);

    public static Failure Fail(string message) => new(message);

    public static implicit operator Result(Failure failure) => new(false, failure.Message);
}

/// <summary>Резултат от операция с върната стойност при успех.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value)
        : base(true, string.Empty)
    {
        _value = value;
    }

    private Result(string error)
        : base(false, error)
    {
    }

    /// <summary>Стойността при успех. Достъпът при неуспех е програмна грешка.</summary>
    public T Value => Succeeded
        ? _value!
        : throw new InvalidOperationException("Резултатът е неуспешен и няма стойност: " + Error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Failure failure) => new(failure.Message);
}
