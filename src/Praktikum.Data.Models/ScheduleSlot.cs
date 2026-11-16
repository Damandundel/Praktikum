using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Седмичен работен слот от графика на назначаването (например понеделник 09:00-13:00).</summary>
public class ScheduleSlot : BaseEntity
{
    public int PlacementId { get; set; }

    public Placement Placement { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
