using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Оценка на назначаване по един критерий, поставена от наставника.</summary>
public class Evaluation : BaseEntity
{
    public int PlacementId { get; set; }

    public Placement Placement { get; set; } = null!;

    public int EvaluationCriterionId { get; set; }

    public EvaluationCriterion EvaluationCriterion { get; set; } = null!;

    [Required]
    public string EvaluatorId { get; set; } = string.Empty;

    public ApplicationUser Evaluator { get; set; } = null!;

    public int Score { get; set; }

    [MaxLength(GlobalConstants.Lengths.ShortText)]
    public string? Comment { get; set; }

    public DateTime EvaluatedOn { get; set; } = DateTime.UtcNow;
}
