using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Praktikum.Common;
using Praktikum.Data.Models;
using Praktikum.Data.Models.Enums;
using Praktikum.Data.Repositories;
using Praktikum.Services.Models;

namespace Praktikum.Services;

public class PositionService : IPositionService
{
    private const string NotFoundMessage = "Позицията не е намерена.";

    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public PositionService(IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    private DateOnly Today => DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

    /// <summary>Проекция към списъчен модел; изпълнява се в SQL, без да се зареждат цели entities.</summary>
    private static readonly Expression<Func<InternshipPosition, PositionListItem>> ToListItem = p => new PositionListItem
    {
        Id = p.Id,
        Title = p.Title,
        OrganizationName = p.Organization.Name,
        PeriodName = p.InternshipPeriod.Name,
        ApplicationDeadline = p.InternshipPeriod.ApplicationDeadline,
        Capacity = p.Capacity,
        FreeSpots = p.Capacity - p.Applications.Count(a => a.Status == ApplicationStatus.Approved),
        ApplicationsCount = p.Applications.Count,
        IsOpen = p.IsOpen,
    };

    public async Task<IReadOnlyList<PositionListItem>> GetOpenAsync(int? periodId, string? search)
    {
        var today = Today;

        var query = _unitOfWork.Repository<InternshipPosition>().AllAsNoTracking()
            .Where(p => p.IsOpen
                        && p.Organization.IsActive
                        && p.InternshipPeriod.IsActive
                        && p.InternshipPeriod.ApplicationDeadline >= today);

        if (periodId.HasValue)
        {
            query = query.Where(p => p.InternshipPeriodId == periodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Contains става параметризирана LIKE заявка - няма SQL Injection.
            var term = search.Trim();
            query = query.Where(p => p.Title.Contains(term) || p.Organization.Name.Contains(term));
        }

        return await query
            .OrderBy(p => p.InternshipPeriod.ApplicationDeadline)
            .ThenBy(p => p.Title)
            .Select(ToListItem)
            .ToListAsync();
    }

    public async Task<PositionDetails?> GetDetailsAsync(int id)
    {
        var row = await _unitOfWork.Repository<InternshipPosition>().AllAsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                Item = new PositionListItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    OrganizationName = p.Organization.Name,
                    PeriodName = p.InternshipPeriod.Name,
                    ApplicationDeadline = p.InternshipPeriod.ApplicationDeadline,
                    Capacity = p.Capacity,
                    FreeSpots = p.Capacity - p.Applications.Count(a => a.Status == ApplicationStatus.Approved),
                    ApplicationsCount = p.Applications.Count,
                    IsOpen = p.IsOpen,
                },
                p.Description,
                OrganizationDescription = p.Organization.Description,
                OrganizationActive = p.Organization.IsActive,
                PeriodActive = p.InternshipPeriod.IsActive,
                p.InternshipPeriod.StartDate,
                p.InternshipPeriod.EndDate,
            })
            .FirstOrDefaultAsync();

        if (row is null)
        {
            return null;
        }

        return new PositionDetails
        {
            Id = row.Item.Id,
            Title = row.Item.Title,
            OrganizationName = row.Item.OrganizationName,
            PeriodName = row.Item.PeriodName,
            ApplicationDeadline = row.Item.ApplicationDeadline,
            Capacity = row.Item.Capacity,
            FreeSpots = row.Item.FreeSpots,
            ApplicationsCount = row.Item.ApplicationsCount,
            IsOpen = row.Item.IsOpen,
            Description = row.Description,
            OrganizationDescription = row.OrganizationDescription,
            PeriodStartDate = row.StartDate,
            PeriodEndDate = row.EndDate,
            CanApply = row.Item.IsOpen && row.OrganizationActive && row.PeriodActive
                       && row.Item.ApplicationDeadline >= Today,
        };
    }

    public async Task<IReadOnlyList<PositionListItem>> GetByOrganizationAsync(int organizationId)
    {
        return await _unitOfWork.Repository<InternshipPosition>().AllAsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderByDescending(p => p.CreatedOn)
            .Select(ToListItem)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PeriodOption>> GetActivePeriodsAsync()
    {
        return await _unitOfWork.Repository<InternshipPeriod>().AllAsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.StartDate)
            .Select(p => new PeriodOption { Id = p.Id, Name = p.Name })
            .ToListAsync();
    }

    public async Task<PositionInput?> GetInputAsync(int organizationId, int id)
    {
        return await _unitOfWork.Repository<InternshipPosition>().AllAsNoTracking()
            .Where(p => p.Id == id && p.OrganizationId == organizationId)
            .Select(p => new PositionInput
            {
                Title = p.Title,
                Description = p.Description,
                Capacity = p.Capacity,
                InternshipPeriodId = p.InternshipPeriodId,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Result<int>> CreateAsync(int organizationId, PositionInput input)
    {
        var periodCheck = await EnsurePeriodIsActiveAsync(input.InternshipPeriodId);
        if (periodCheck.Failed)
        {
            return periodCheck.Failure;
        }

        var position = new InternshipPosition
        {
            OrganizationId = organizationId,
            InternshipPeriodId = input.InternshipPeriodId,
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            Capacity = input.Capacity,
            CreatedOn = _timeProvider.GetUtcNow().UtcDateTime,
        };

        await _unitOfWork.Repository<InternshipPosition>().AddAsync(position);
        await _unitOfWork.SaveChangesAsync();
        return position.Id;
    }

    public async Task<Result> UpdateAsync(int organizationId, int id, PositionInput input)
    {
        var found = await GetOwnedPositionAsync(organizationId, id);
        if (found.Failed)
        {
            return found.Failure;
        }

        var position = found.Value;

        var approvedCount = await CountApprovedAsync(id);
        if (input.Capacity < approvedCount)
        {
            return Result.Fail(
                $"Капацитетът не може да е под броя на одобрените ученици ({approvedCount}).");
        }

        if (input.InternshipPeriodId != position.InternshipPeriodId)
        {
            // Смяната на период би променила датите на вече подадени кандидатури.
            var hasApplications = await _unitOfWork.Repository<Application>().AllAsNoTracking()
                .AnyAsync(a => a.InternshipPositionId == id);
            if (hasApplications)
            {
                return Result.Fail("Периодът не може да се променя, след като има подадени кандидатури.");
            }

            var periodCheck = await EnsurePeriodIsActiveAsync(input.InternshipPeriodId);
            if (periodCheck.Failed)
            {
                return periodCheck.Failure;
            }

            position.InternshipPeriodId = input.InternshipPeriodId;
        }

        position.Title = input.Title.Trim();
        position.Description = input.Description.Trim();
        position.Capacity = input.Capacity;

        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> SetOpenAsync(int organizationId, int id, bool isOpen)
    {
        var found = await GetOwnedPositionAsync(organizationId, id);
        if (found.Failed)
        {
            return found.Failure;
        }

        found.Value.IsOpen = isOpen;
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int organizationId, int id)
    {
        var found = await GetOwnedPositionAsync(organizationId, id);
        if (found.Failed)
        {
            return found.Failure;
        }

        var position = found.Value;

        var hasApplications = await _unitOfWork.Repository<Application>().AllAsNoTracking()
            .AnyAsync(a => a.InternshipPositionId == id);
        if (hasApplications)
        {
            return Result.Fail(
                "Позиция с подадени кандидатури не може да се изтрие. Затворете я за нови кандидатури.");
        }

        _unitOfWork.Repository<InternshipPosition>().Delete(position);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    /// <summary>Връща позицията само ако е на дадената организация (работодателят не пипа чужди позиции).</summary>
    private async Task<Result<InternshipPosition>> GetOwnedPositionAsync(int organizationId, int id)
    {
        var position = await _unitOfWork.Repository<InternshipPosition>().All()
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == organizationId);

        return position is null ? Result.Fail(NotFoundMessage) : position;
    }

    private async Task<Result> EnsurePeriodIsActiveAsync(int periodId)
    {
        var isActive = await _unitOfWork.Repository<InternshipPeriod>().AllAsNoTracking()
            .AnyAsync(p => p.Id == periodId && p.IsActive);

        if (!isActive)
        {
            return Result.Fail("Избраният период не съществува или не е активен.");
        }

        return Result.Ok();
    }

    private Task<int> CountApprovedAsync(int positionId)
    {
        return _unitOfWork.Repository<Application>().AllAsNoTracking()
            .CountAsync(a => a.InternshipPositionId == positionId && a.Status == ApplicationStatus.Approved);
    }
}
