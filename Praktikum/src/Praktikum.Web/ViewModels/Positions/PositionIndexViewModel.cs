using Praktikum.Services.Models;

namespace Praktikum.Web.ViewModels.Positions;

/// <summary>Списък с позиции и филтрите му.</summary>
public class PositionIndexViewModel
{
    public int? PeriodId { get; set; }

    public string? Search { get; set; }

    public IReadOnlyList<PeriodOption> Periods { get; set; } = Array.Empty<PeriodOption>();

    public IReadOnlyList<PositionListItem> Positions { get; set; } = Array.Empty<PositionListItem>();
}
