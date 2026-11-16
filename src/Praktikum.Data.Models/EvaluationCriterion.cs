using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Критерий за оценяване (например "Отговорност", "Технически умения").</summary>
public class EvaluationCriterion : BaseEntity
{
    [Required]
    [MaxLength(GlobalConstants.Lengths.Name)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(GlobalConstants.Lengths.ShortText)]
    public string? Description { get; set; }

    /// <summary>Максимален брой точки по критерия.</summary>
    public int MaxScore { get; set; } = 6;

    public bool IsActive { get; set; } = true;

    public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
}
