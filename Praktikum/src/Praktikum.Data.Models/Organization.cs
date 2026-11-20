using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Организация (фирма, институция), която предлага стажантски позиции.</summary>
public class Organization : BaseEntity
{
    [Required]
    [MaxLength(GlobalConstants.Lengths.Name)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string? Description { get; set; }

    [MaxLength(GlobalConstants.Lengths.ShortText)]
    public string? Address { get; set; }

    [Required]
    [MaxLength(GlobalConstants.Lengths.Email)]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(GlobalConstants.Lengths.Phone)]
    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();

    public ICollection<InternshipPosition> Positions { get; set; } = new HashSet<InternshipPosition>();
}
