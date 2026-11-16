namespace Praktikum.Data.Seeding;

/// <summary>Една стъпка от началното попълване на базата (роли, потребители, данни...).</summary>
public interface ISeeder
{
    Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider);
}
