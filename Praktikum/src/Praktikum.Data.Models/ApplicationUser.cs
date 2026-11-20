using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Praktikum.Common;

namespace Praktikum.Data.Models;

/// <summary>
/// Потребител на системата. Една и съща таблица обслужва всички роли;
/// ролята се пази в Identity, а OrganizationId е попълнено само за работодатели и наставници.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(GlobalConstants.Lengths.PersonName)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(GlobalConstants.Lengths.PersonName)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Клас, например "12А". Само за ученици.</summary>
    [MaxLength(GlobalConstants.Lengths.ClassName)]
    public string? ClassName { get; set; }

    public int? OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    /// <summary>Кратко професионално заглавие, например "Ученик, интересуващ се от уеб разработка".</summary>
    [MaxLength(GlobalConstants.Lengths.Title)]
    public string? Headline { get; set; }

    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string? About { get; set; }

    /// <summary>Къде учи в момента.</summary>
    [MaxLength(GlobalConstants.Lengths.Name)]
    public string? School { get; set; }

    [MaxLength(GlobalConstants.Lengths.Name)]
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProfileEntry> ProfileEntries { get; set; } = new HashSet<ProfileEntry>();

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Application> Applications { get; set; } = new HashSet<Application>();

    public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
}
