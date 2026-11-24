using Microsoft.AspNetCore.Mvc;
using Praktikum.Services;
using Praktikum.Web.ViewModels.Positions;

namespace Praktikum.Web.Controllers;

/// <summary>Разглеждане на отворените стажантски позиции (за всеки влязъл потребител).</summary>
public class PositionsController : Controller
{
    private readonly IPositionService _positionService;

    public PositionsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? periodId, string? search)
    {
        var model = new PositionIndexViewModel
        {
            PeriodId = periodId,
            Search = search,
            Periods = await _positionService.GetActivePeriodsAsync(),
            Positions = await _positionService.GetOpenAsync(periodId, search),
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var position = await _positionService.GetDetailsAsync(id);
        return position is null ? NotFound() : View(position);
    }
}
