using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Praktikum.Common;

namespace Praktikum.Data.Seeding;

/// <summary>Създава петте роли на системата, ако липсват.</summary>
public class RolesSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roles = new[]
        {
            GlobalConstants.Roles.Administrator,
            GlobalConstants.Roles.Coordinator,
            GlobalConstants.Roles.Employer,
            GlobalConstants.Roles.Mentor,
            GlobalConstants.Roles.Student,
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Ролята '{role}' не можа да бъде създадена: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
