using System.Security.Claims;

namespace Praktikum.Web.Infrastructure;

/// <summary>Удобни методи за четене на данните на влезлия потребител от бисквитката.</summary>
public static class ClaimsPrincipalExtensions
{
    public const string OrganizationIdClaimType = "OrganizationId";

    public static string GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? throw new InvalidOperationException("Потребителят не е автентикиран.");

    /// <summary>Идентификатор на организацията (само за работодатели и наставници).</summary>
    public static int? GetOrganizationId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirstValue(OrganizationIdClaimType), out var id) ? id : null;
}
