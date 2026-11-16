using System.ComponentModel.DataAnnotations;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Запис в професионалния профил: образование, стаж, работа или постижение.</summary>
public class ProfileEntry : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public ProfileEntryType Type { get; set; }

    [Required]
    [MaxLength(GlobalConstants.Lengths.Title)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Училище, фирма или организация, свързана със записа.</summary>
    [MaxLength(GlobalConstants.Lengths.Name)]
    public string? Organization { get; set; }

    [MaxLength(GlobalConstants.Lengths.ShortText)]
    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
