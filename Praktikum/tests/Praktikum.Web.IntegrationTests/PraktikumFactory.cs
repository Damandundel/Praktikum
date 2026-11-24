using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Praktikum.Data;

namespace Praktikum.Web.IntegrationTests;

/// <summary>
/// Стартира цялото приложение в паметта на теста (истински middleware, контролери, Identity и сидъри),
/// но вместо SQL Server ползва SQLite в паметта. Всяка фабрика има своя изолирана база.
/// </summary>
public sealed class PraktikumFactory : WebApplicationFactory<Program>
{
    // Базата в паметта живее, докато връзката е отворена, затова я държим отворена до края на теста.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public PraktikumFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" не е Development, така че User Secrets (реалният connection string) не се зареждат.
        builder.UseEnvironment("Testing");

        // Program.cs изисква connection string; стойността е фиктивна и се заменя по-долу.
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=unused;Database=unused");
        builder.UseSetting("Database:ApplyMigrations", "false");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, DecimalAsDoubleModelCustomizer>());
        });
    }

    /// <summary>HTTP клиент към приложението през HTTPS (бисквитката е Secure), без автоматични пренасочвания.</summary>
    public AppClient CreateAppClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

        return new AppClient(client);
    }

    /// <summary>Изпълнява код с услугите на приложението (например за проверка на базата).</summary>
    public async Task<T> WithDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    /// <summary>
    /// SQLite не поддържа SUM върху decimal. В тестовете decimal се съхранява като double,
    /// така че същите LINQ заявки да се изпълняват и върху SQLite. В SQL Server (продукция) няма промяна.
    /// </summary>
    private sealed class DecimalAsDoubleModelCustomizer : ModelCustomizer
    {
        public DecimalAsDoubleModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                             .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
                {
                    modelBuilder.Entity(entityType.ClrType).Property(property.Name).HasConversion<double>();
                }
            }
        }
    }
}
