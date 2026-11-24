using Microsoft.EntityFrameworkCore;
using Praktikum.Common;
using Praktikum.Services.Models;

namespace Praktikum.Services.Tests;

public class PositionServiceTests
{
    [Fact]
    public async Task GetOpen_ReturnsOnlyPositionsThatAcceptApplications()
    {
        using var db = new TestDatabase();
        var organization = db.AddOrganization();
        var openPeriod = db.AddPeriod();
        var expiredPeriod = db.AddPeriod(deadline: TestDatabase.Today.AddDays(-1));
        var inactivePeriod = db.AddPeriod(isActive: false);

        var open = db.AddPosition(organization, openPeriod);
        db.AddPosition(organization, openPeriod, isOpen: false);
        db.AddPosition(organization, expiredPeriod);
        db.AddPosition(organization, inactivePeriod);
        db.AddPosition(db.AddOrganization("Неактивна ООД", isActive: false), openPeriod);

        var result = await db.CreatePositionService().GetOpenAsync(null, null);

        Assert.Single(result);
        Assert.Equal(open.Id, result[0].Id);
    }

    [Fact]
    public async Task GetOpen_SearchMatchesTitleOrOrganization_AndIgnoresSqlLikePayload()
    {
        using var db = new TestDatabase();
        var period = db.AddPeriod();
        var first = db.AddPosition(db.AddOrganization("Алфа"), period);
        first.Title = "Уеб разработчик";
        db.AddPosition(db.AddOrganization("Бета"), period);
        db.Context.SaveChanges();
        var service = db.CreatePositionService();

        // Точен регистър, защото InMemory (за разлика от SQL Server) различава главни и малки букви.
        Assert.Single(await service.GetOpenAsync(null, "Уеб"));
        Assert.Single(await service.GetOpenAsync(null, "Алфа"));
        Assert.Empty(await service.GetOpenAsync(null, "'; DROP TABLE InternshipPositions; --"));
    }

    [Fact]
    public async Task GetDetails_AfterDeadline_CannotApply()
    {
        using var db = new TestDatabase();
        var position = db.AddPosition(db.AddOrganization(), db.AddPeriod(deadline: TestDatabase.Today.AddDays(-1)));

        var details = await db.CreatePositionService().GetDetailsAsync(position.Id);

        Assert.NotNull(details);
        Assert.False(details!.CanApply);
    }

    [Fact]
    public async Task GetDetails_ForMissingPosition_ReturnsNull()
    {
        using var db = new TestDatabase();

        Assert.Null(await db.CreatePositionService().GetDetailsAsync(123));
    }

    [Fact]
    public async Task Create_InInactivePeriod_Throws()
    {
        using var db = new TestDatabase();
        var organization = db.AddOrganization();
        var period = db.AddPeriod(isActive: false);

        await ResultAssert.FailsAsync(db.CreatePositionService().CreateAsync(
            organization.Id,
            new PositionInput { Title = "Тест", Description = "Описание", Capacity = 1, InternshipPeriodId = period.Id }));
    }

    [Fact]
    public async Task Create_TrimsTextAndAttachesToOrganization()
    {
        using var db = new TestDatabase();
        var organization = db.AddOrganization();
        var period = db.AddPeriod();

        var id = await ResultAssert.SucceedsAsync(db.CreatePositionService().CreateAsync(
            organization.Id,
            new PositionInput { Title = "  Тест  ", Description = " Описание ", Capacity = 2, InternshipPeriodId = period.Id }));

        var position = await db.Context.InternshipPositions.SingleAsync(p => p.Id == id);
        Assert.Equal("Тест", position.Title);
        Assert.Equal("Описание", position.Description);
        Assert.Equal(organization.Id, position.OrganizationId);
    }

    [Fact]
    public async Task Update_PositionOfAnotherOrganization_Throws()
    {
        using var db = new TestDatabase();
        var owner = db.AddOrganization("Собственик");
        var intruder = db.AddOrganization("Натрапник");
        var period = db.AddPeriod();
        var position = db.AddPosition(owner, period);

        await ResultAssert.FailsAsync(db.CreatePositionService().UpdateAsync(
            intruder.Id,
            position.Id,
            new PositionInput { Title = "Хакнато", Description = "Описание", Capacity = 1, InternshipPeriodId = period.Id }));
    }

    [Fact]
    public async Task SetOpen_ClosesPositionForApplications()
    {
        using var db = new TestDatabase();
        var organization = db.AddOrganization();
        var position = db.AddPosition(organization, db.AddPeriod());
        var service = db.CreatePositionService();

        await ResultAssert.SucceedsAsync(service.SetOpenAsync(organization.Id, position.Id, false));

        Assert.Empty(await service.GetOpenAsync(null, null));
    }

    [Fact]
    public async Task Delete_PositionWithoutApplications_RemovesIt()
    {
        using var db = new TestDatabase();
        var organization = db.AddOrganization();
        var position = db.AddPosition(organization, db.AddPeriod());

        await ResultAssert.SucceedsAsync(db.CreatePositionService().DeleteAsync(organization.Id, position.Id));

        Assert.Empty(db.Context.InternshipPositions);
    }
}
