using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Кандидатура на ученик за стажантска позиция.</summary>
public class Application : BaseEntity
{
    [Required]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser Student { get; set; } = null!;

    public int InternshipPositionId { get; set; }

    public InternshipPosition InternshipPosition { get; set; } = null!;

    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string? MotivationLetter { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedOn { get; set; }

    [MaxLength(GlobalConstants.Lengths.ShortText)]
    public string? ReviewComment { get; set; }

    /// <summary>Назначаването, създадено след одобрение (ако има такова).</summary>
    public Placement? Placement { get; set; }
}
