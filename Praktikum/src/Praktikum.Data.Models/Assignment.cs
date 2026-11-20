using System.ComponentModel.DataAnnotations;
using Praktikum.Common;

namespace Praktikum.Data.Models;

/// <summary>Задание към назначен стажант (като в Classroom): описание, срок и максимални точки.</summary>
public class Assignment : BaseEntity
{
    public int PlacementId { get; set; }

    public Placement Placement { get; set; } = null!;

    [Required]
    [MaxLength(GlobalConstants.Lengths.Title)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Краен срок (UTC). Предаване след него е със закъснение и носи отстъпка от точките.</summary>
    public DateTime DueOn { get; set; }

    public int MaxPoints { get; set; } = 100;

    [Required]
    public string CreatedById { get; set; } = string.Empty;

    public ApplicationUser CreatedBy { get; set; } = null!;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
}
