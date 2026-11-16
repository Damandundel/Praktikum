using System.ComponentModel.DataAnnotations;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Предадена работа по задание: текст и/или файл, прегледана от наставника.</summary>
public class Submission : BaseEntity
{
    public int AssignmentId { get; set; }

    public Assignment Assignment { get; set; } = null!;

    [Required]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser Student { get; set; } = null!;

    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string? Text { get; set; }

    /// <summary>Оригиналното име на файла (за показване и сваляне).</summary>
    [MaxLength(GlobalConstants.Lengths.Title)]
    public string? FileName { get; set; }

    /// <summary>Името на файла върху диска (случайно), извън публичната папка wwwroot.</summary>
    [MaxLength(GlobalConstants.Lengths.Title)]
    public string? StoredFileName { get; set; }

    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

    public bool IsLate { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    /// <summary>Точки, дадени от наставника (преди отстъпката за закъснение).</summary>
    public int? Points { get; set; }

    [MaxLength(GlobalConstants.Lengths.LongText)]
    public string? Feedback { get; set; }

    public string? ReviewerId { get; set; }

    public ApplicationUser? Reviewer { get; set; }

    public DateTime? ReviewedOn { get; set; }
}
