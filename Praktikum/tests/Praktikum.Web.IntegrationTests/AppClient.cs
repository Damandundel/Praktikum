using System.Net;
using System.Text.RegularExpressions;
using Praktikum.Data.Seeding;

namespace Praktikum.Web.IntegrationTests;

/// <summary>Отговор на сървъра във форма, удобна за проверки в тестовете.</summary>
public sealed record AppResponse(HttpStatusCode Status, string Body, string? Location, HttpResponseMessage Raw)
{
    public bool IsRedirect => (int)Status is >= 300 and < 400;

    /// <summary>Пренасочване към страницата за вход.</summary>
    public bool RedirectsToLogin => IsRedirect && Location?.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>Пренасочване към страницата "403 - нямате достъп" (така работи AccessDeniedPath).</summary>
    public bool RedirectsToForbidden => IsRedirect && Location?.Contains("/Error/403", StringComparison.OrdinalIgnoreCase) == true;
}

/// <summary>
/// Прави това, което прави браузърът: пази бисквитките, взема антифалшификационния токен от страницата
/// и го връща с формата. Така тестовете минават през същия път като реален потребител.
/// </summary>
public sealed partial class AppClient : IDisposable
{
    private readonly HttpClient _http;

    public AppClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AppResponse> GetAsync(string url)
    {
        return await ToResponseAsync(await _http.GetAsync(url));
    }

    /// <summary>
    /// Изпраща форма. Токенът се взема от страницата <paramref name="tokenPage"/> (по подразбиране същия адрес),
    /// защото токенът е свързан с бисквитката на този клиент.
    /// </summary>
    public async Task<AppResponse> PostFormAsync(
        string url, IDictionary<string, string> fields, string? tokenPage = null, bool withToken = true)
    {
        var data = new Dictionary<string, string>(fields);

        if (withToken)
        {
            var page = await GetAsync(tokenPage ?? url);
            data["__RequestVerificationToken"] = ExtractToken(page.Body)
                ?? throw new InvalidOperationException($"Страницата {tokenPage ?? url} не съдържа антифалшификационен токен.");
        }

        return await ToResponseAsync(await _http.PostAsync(url, new FormUrlEncodedContent(data)));
    }

    /// <summary>Следва пренасочването от отговора и връща целевата страница.</summary>
    public async Task<AppResponse> FollowAsync(AppResponse response)
    {
        Assert.True(response.IsRedirect, $"Очаквано пренасочване, получено {(int)response.Status}.");
        return await GetAsync(response.Location!);
    }

    public async Task<AppResponse> LoginAsync(string email, string password = DemoAccounts.Password)
    {
        return await PostFormAsync(
            "/Account/Login", new Dictionary<string, string> { ["Email"] = email, ["Password"] = password });
    }

    public async Task<AppClient> WithLoginAsync(string email)
    {
        var response = await LoginAsync(email);
        Assert.True(response.IsRedirect, $"Входът с {email} не успя: {(int)response.Status}.");
        return this;
    }

    public static string? ExtractToken(string html)
    {
        var match = TokenRegex().Match(html);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>Идентификаторите от адреси като /controller/action/{id}, в реда на появяване.</summary>
    public static IReadOnlyList<int> ExtractIds(string html, string urlPrefix)
    {
        return Regex.Matches(html, Regex.Escape(urlPrefix) + @"(\d+)")
            .Select(m => int.Parse(m.Groups[1].Value))
            .Distinct()
            .ToList();
    }

    public void Dispose() => _http.Dispose();

    private static async Task<AppResponse> ToResponseAsync(HttpResponseMessage message)
    {
        var body = await message.Content.ReadAsStringAsync();
        return new AppResponse(message.StatusCode, body, message.Headers.Location?.ToString(), message);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}
