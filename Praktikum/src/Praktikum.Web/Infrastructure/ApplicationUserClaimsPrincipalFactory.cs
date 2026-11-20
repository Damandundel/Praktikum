using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Praktikum.Data.Models;

namespace Praktikum.Web.Infrastructure;

/// <summary>
/// Добавя името на потребителя в бисквитката (claims), за да го показваме
/// в менюто без допълнителна заявка към базата при всяка страница.
/// </summary>
public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
        identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

        if (user.OrganizationId.HasValue)
        {
            identity.AddClaim(new Claim(
                ClaimsPrincipalExtensions.OrganizationIdClaimType,
                user.OrganizationId.Value.ToString()));
        }
        return identity;
    }
}
