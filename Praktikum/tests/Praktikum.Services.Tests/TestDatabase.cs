using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Praktikum.Data;
using Praktikum.Data.Models;
using Praktikum.Data.Repositories;

namespace Praktikum.Services.Tests;

/// <summary>
/// Помощен клас за тестовете: чиста база в паметта за всеки тест, фиксирано "днес"
/// и методи за бързо създаване на данни. Така тестовете не зависят от SQL Server и от часовника.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    public TestDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        UnitOfWork = new UnitOfWork(Context);
        Time = new FakeTimeProvider(new DateTimeOffset(Today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero));
    }

    /// <summary>Фиксираната дата "днес" във всички тестове.</summary>
    public static DateOnly Today { get; } = new(2027, 3, 1);

    public ApplicationDbContext Context { get; }

    public IUnitOfWork UnitOfWork { get; }

    public FakeTimeProvider Time { get; }

    public ApplicationUser AddStudent(string id = "student-1")
    {
        var student = new ApplicationUser { Id = id, UserName = id, FirstName = "Иван", LastName = "Петров", ClassName = "12А" };
        Context.Users.Add(student);
        Context.SaveChanges();
        return student;
    }

    public Organization AddOrganization(string name = "Тест ООД", bool isActive = true)
    {
        var organization = new Organization { Name = name, ContactEmail = "test@test.demo", IsActive = isActive };
        Context.Organizations.Add(organization);
        Context.SaveChanges();
        return organization;
    }

    /// <summary>По подразбиране период, който започва след 30 дни и има срок за кандидатстване след 10 дни.</summary>
    public InternshipPeriod AddPeriod(
        DateOnly? start = null, DateOnly? end = null, DateOnly? deadline = null, bool isActive = true)
    {
        var period = new InternshipPeriod
        {
            Name = "Период " + Guid.NewGuid().ToString("N")[..6],
            StartDate = start ?? Today.AddDays(30),
            EndDate = end ?? Today.AddDays(60),
            ApplicationDeadline = deadline ?? Today.AddDays(10),
            RequiredHours = 80,
            IsActive = isActive,
        };
        Context.InternshipPeriods.Add(period);
        Context.SaveChanges();
        return period;
    }

    public InternshipPosition AddPosition(
        Organization organization, InternshipPeriod period, int capacity = 2, bool isOpen = true)
    {
        var position = new InternshipPosition
        {
            Title = "Позиция " + Guid.NewGuid().ToString("N")[..6],
            Description = "Описание",
            Capacity = capacity,
            IsOpen = isOpen,
            OrganizationId = organization.Id,
            InternshipPeriodId = period.Id,
        };
        Context.InternshipPositions.Add(position);
        Context.SaveChanges();
        return position;
    }

    public PositionService CreatePositionService() => new(UnitOfWork, Time);

    public void Dispose() => Context.Dispose();
}
