namespace Praktikum.Data.Seeding;

/// <summary>Изпълнява всички сидъри по ред (ролите са преди потребителите).</summary>
public class ApplicationDbContextSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
    {
        var seeders = new ISeeder[]
        {
            new RolesSeeder(),
            new DemoDataSeeder(),
        };

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(dbContext, serviceProvider);
        }
    }
}
