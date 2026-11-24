using Praktikum.Common;

namespace Praktikum.Services.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_Succeeds_WithEmptyError()
    {
        var result = Result.Ok();

        Assert.True(result.Succeeded);
        Assert.False(result.Failed);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Fail_Fails_WithMessage()
    {
        Result result = Result.Fail("Грешка.");

        Assert.True(result.Failed);
        Assert.Equal("Грешка.", result.Error);
    }

    [Fact]
    public void ValueResult_FromValue_Succeeds()
    {
        Result<int> result = 42;

        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ValueResult_FromFailure_FailsAndKeepsMessage()
    {
        Result<int> result = Result.Fail("Няма такава.");

        Assert.True(result.Failed);
        Assert.Equal("Няма такава.", result.Error);
    }

    [Fact]
    public void ValueResult_ReadingValueOfFailure_IsAProgrammingError()
    {
        Result<string> result = Result.Fail("Грешка.");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_CanBePassedAcrossResultTypes()
    {
        Result<int> inner = Result.Fail("Вътрешна грешка.");

        Result<string> outer = inner.Failure;

        Assert.True(outer.Failed);
        Assert.Equal("Вътрешна грешка.", outer.Error);
    }
}
