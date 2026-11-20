using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Стажантска позиция, публикувана от работодател за конкретен период.</summary>
public class InternshipPosition : BaseEntity
{
    [Required]
    [MaxLength(GlobalConstants.Lengths.Title)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Максимален брой ученици за позицията.</summary>
    public int Capacity { get; set; }

    /// <summary>Позицията приема ли кандидатури.</summary>
    public bool IsOpen { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public int InternshipPeriodId { get; set; }

    public InternshipPeriod InternshipPeriod { get; set; } = null!;

    public ICollection<Application> Applications { get; set; } = new HashSet<Application>();
}
