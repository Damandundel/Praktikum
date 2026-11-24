using Praktikum.Common;
using Praktikum.Services.Models;

namespace Praktikum.Services;

/// <summary>Стажантски позиции: разглеждане от учениците и управление от работодателите.</summary>
public interface IPositionService
{
    /// <summary>Позиции, за които в момента може да се кандидатства (по период и/или ключова дума).</summary>
    Task<IReadOnlyList<PositionListItem>> GetOpenAsync(int? periodId, string? search);

    Task<PositionDetails?> GetDetailsAsync(int id);

    Task<IReadOnlyList<PositionListItem>> GetByOrganizationAsync(int organizationId);

    Task<IReadOnlyList<PeriodOption>> GetActivePeriodsAsync();

    /// <summary>Данни за формата за редакция. Връща null, ако позицията не е на тази организация.</summary>
    Task<PositionInput?> GetInputAsync(int organizationId, int id);

    Task<Result<int>> CreateAsync(int organizationId, PositionInput input);

    Task<Result> UpdateAsync(int organizationId, int id, PositionInput input);

    Task<Result> SetOpenAsync(int organizationId, int id, bool isOpen);

    Task<Result> DeleteAsync(int organizationId, int id);
}
