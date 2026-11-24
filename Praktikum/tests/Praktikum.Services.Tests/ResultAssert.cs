using Praktikum.Common;

namespace Praktikum.Services.Tests;

/// <summary>Помощни проверки за резултати от услугите (вместо Assert.ThrowsAsync за бизнес правила).</summary>
public static class ResultAssert
{
    /// <summary>Очаква неуспех и връща съобщението за грешка.</summary>
    public static async Task<string> FailsAsync(Task<Result> task)
    {
        var result = await task;
        Assert.True(result.Failed, "Очакваше се неуспех, но операцията е успяла.");
        return result.Error;
    }

    /// <summary>Очаква неуспех и връща съобщението за грешка.</summary>
    public static async Task<string> FailsAsync<T>(Task<Result<T>> task)
    {
        var result = await task;
        Assert.True(result.Failed, "Очакваше се неуспех, но операцията е успяла.");
        return result.Error;
    }

    /// <summary>Очаква успех (при неуспех тестът показва съобщението за грешка).</summary>
    public static async Task SucceedsAsync(Task<Result> task)
    {
        var result = await task;
        Assert.True(result.Succeeded, result.Error);
    }

    /// <summary>Очаква успех и връща стойността.</summary>
    public static async Task<T> SucceedsAsync<T>(Task<Result<T>> task)
    {
        var result = await task;
        Assert.True(result.Succeeded, result.Error);
        return result.Value;
    }
}
