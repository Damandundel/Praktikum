namespace Praktikum.Services.Models;

/// <summary>Позиция в списък (само нужните за показване полета).</summary>
public class PositionListItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string PeriodName { get; set; } = string.Empty;

    public DateOnly ApplicationDeadline { get; set; }

    public int Capacity { get; set; }

    /// <summary>Свободни места = капацитет минус одобрените кандидатури.</summary>
    public int FreeSpots { get; set; }

    public int ApplicationsCount { get; set; }

    public bool IsOpen { get; set; }
}

/// <summary>Пълни данни за една позиция.</summary>
public class PositionDetails : PositionListItem
{
    public string Description { get; set; } = string.Empty;

    public string? OrganizationDescription { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    /// <summary>Може ли в момента да се кандидатства (отворена, активен период, срокът не е изтекъл).</summary>
    public bool CanApply { get; set; }
}

/// <summary>Данни, въведени от работодателя при създаване или редакция на позиция.</summary>
public class PositionInput
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Capacity { get; set; } = 1;

    public int InternshipPeriodId { get; set; }
}

public class PeriodOption
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
