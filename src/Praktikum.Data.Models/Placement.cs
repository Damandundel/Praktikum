using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Назначаване: одобрен ученик на позиция с наставник и график.</summary>
public class Placement : BaseEntity
{
    public int ApplicationId { get; set; }

    public Application Application { get; set; } = null!;

    /// <summary>Дублира се от кандидатурата, за да може базата да налага уникалност с индекс.</summary>
    [Required]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser Student { get; set; } = null!;

    /// <summary>Дублира се от позицията по същата причина като StudentId.</summary>
    public int InternshipPeriodId { get; set; }

    public InternshipPeriod InternshipPeriod { get; set; } = null!;

    [Required]
    public string MentorId { get; set; } = string.Empty;

    public ApplicationUser Mentor { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public PlacementStatus Status { get; set; } = PlacementStatus.Planned;

    /// <summary>Описание на свършената работа, написано от наставника при приключване.</summary>
    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string? CompletionSummary { get; set; }

    /// <summary>Крайна оценка по шестобалната система (2.00 - 6.00), поставена от наставника.</summary>
    public decimal? FinalGrade { get; set; }

    public DateTime? CompletedOn { get; set; }

    public HiringDecision HiringDecision { get; set; } = HiringDecision.NotDecided;

    [MaxLength(GlobalConstants.Lengths.ShortText)]
    public string? HiringMessage { get; set; }

    public DateTime? HiringDecidedOn { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new HashSet<Assignment>();

    public ICollection<ScheduleSlot> ScheduleSlots { get; set; } = new HashSet<ScheduleSlot>();

    public ICollection<DiaryEntry> DiaryEntries { get; set; } = new HashSet<DiaryEntry>();

    public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
}
