using System.Net;
using Praktikum.Data.Seeding;

namespace Praktikum.Web.IntegrationTests;

/// <summary>Основата на приложението (Етап 2): стартиране, вход и роли, защита на страниците и страници за грешки.</summary>
public class FoundationTests : IClassFixture<PraktikumFactory>
{
    private readonly PraktikumFactory _factory;

    public FoundationTests(PraktikumFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HomePage_IsPublic_AndShowsBulgarianText()
    {
        using var client = _factory.CreateAppClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains("Система за управление на ученически стажове", response.Body);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Privacy")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    public async Task PublicPages_AreOpenWithoutLogin(string url)
    {
        using var client = _factory.CreateAppClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).Status);
    }

    [Theory]
    [InlineData("/Positions")]
    [InlineData("/Positions/Details/1")]
    public async Task ProtectedPages_RedirectAnonymousVisitorsToLogin(string url)
    {
        using var client = _factory.CreateAppClient();

        var response = await client.GetAsync(url);

        Assert.True(response.RedirectsToLogin, $"{url} -> {(int)response.Status} {response.Location}");
    }

    [Theory]
    [InlineData(DemoAccounts.AdministratorEmail)]
    [InlineData(DemoAccounts.CoordinatorEmail)]
    [InlineData(DemoAccounts.EmployerEmail)]
    [InlineData(DemoAccounts.MentorEmail)]
    [InlineData(DemoAccounts.StudentEmail)]
    public async Task EveryRole_CanLogInAndSeeThePositions(string email)
    {
        using var client = await _factory.CreateAppClient().WithLoginAsync(email);

        var response = await client.GetAsync("/Positions");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains("Позиции", response.Body);
    }

    [Fact]
    public async Task Login_WithWrongPasswordOrUnknownEmail_GivesSameGenericMessage()
    {
        using var client = _factory.CreateAppClient();

        var wrongPassword = await client.LoginAsync(DemoAccounts.StudentEmail, "Wrong#Pass123");
        var unknownEmail = await client.LoginAsync("nobody@praktikum.demo", "Wrong#Pass123");

        Assert.Equal(HttpStatusCode.OK, wrongPassword.Status);
        Assert.Contains("Грешен имейл или парола", wrongPassword.Body);
        Assert.Contains("Грешен имейл или парола", unknownEmail.Body);
    }

    [Fact]
    public async Task LoginCookie_IsHttpOnlySecureAndSameSiteLax()
    {
        using var client = _factory.CreateAppClient();

        var response = await client.LoginAsync(DemoAccounts.StudentEmail);

        var cookie = response.Raw.Headers.GetValues("Set-Cookie")
            .Single(c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginWithoutAntiforgeryToken_IsRejectedWith400()
    {
        using var client = _factory.CreateAppClient();

        var response = await client.PostFormAsync(
            "/Account/Login",
            new Dictionary<string, string> { ["Email"] = DemoAccounts.StudentEmail, ["Password"] = DemoAccounts.Password },
            withToken: false);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task Register_CreatesStudentWhoCanLogIn()
    {
        var email = $"new-{Guid.NewGuid():N}@praktikum.demo";
        using var client = _factory.CreateAppClient();

        var registration = await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["FirstName"] = "Нов", ["LastName"] = "Ученик", ["ClassName"] = "10А",
            ["Email"] = email, ["Password"] = "Strong#Pass1", ["ConfirmPassword"] = "Strong#Pass1",
        });

        Assert.True(registration.IsRedirect, registration.Body);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Positions")).Status);
    }

    [Theory]
    [InlineData("/Error/400", HttpStatusCode.BadRequest)]
    [InlineData("/Error/403", HttpStatusCode.Forbidden)]
    [InlineData("/Error/404", HttpStatusCode.NotFound)]
    [InlineData("/Error/500", HttpStatusCode.InternalServerError)]
    public async Task ErrorPages_AreShownInBulgarian(string url, HttpStatusCode expected)
    {
        using var client = _factory.CreateAppClient();

        var response = await client.GetAsync(url);

        Assert.Equal(expected, response.Status);
        Assert.Contains("Към началото", response.Body);
    }

    [Fact]
    public async Task UnknownAddress_ShowsTheNotFoundPageToSignedInUsers()
    {
        using var client = await _factory.CreateAppClient().WithLoginAsync(DemoAccounts.StudentEmail);

        var response = await client.GetAsync("/няма-такава-страница");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Към началото", response.Body);
    }
}
