using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Запис в дневника на практиката, който наставникът потвърждава.</summary>
public class DiaryEntry : BaseEntity
{
    public int PlacementId { get; set; }

    public Placement Placement { get; set; } = null!;

    public DateOnly Date { get; set; }

    /// <summary>Изработени часове за деня.</summary>
    [Column(TypeName = "decimal(4,2)")]
    public decimal Hours { get; set; }

    [Required]
    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string Activity { get; set; } = string.Empty;

    public DiaryEntryStatus Status { get; set; } = DiaryEntryStatus.Pending;

    public string? ConfirmedById { get; set; }

    public ApplicationUser? ConfirmedBy { get; set; }

    public DateTime? ConfirmedOn { get; set; }

    [MaxLength(GlobalConstants.Lengths.ShortText)]
    public string? MentorComment { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
