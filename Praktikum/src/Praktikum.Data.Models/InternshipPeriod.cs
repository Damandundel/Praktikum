using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Период на провеждане на практиката (например "Лятна практика 2027").</summary>
public class InternshipPeriod : BaseEntity
{
    [Required]
    [MaxLength(GlobalConstants.Lengths.Name)]
    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    /// <summary>Краен срок за кандидатстване.</summary>
    public DateOnly ApplicationDeadline { get; set; }

    /// <summary>Брой часове, които ученикът трябва да изработи през периода.</summary>
    public int RequiredHours { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InternshipPosition> Positions { get; set; } = new HashSet<InternshipPosition>();

    public ICollection<Placement> Placements { get; set; } = new HashSet<Placement>();
}
