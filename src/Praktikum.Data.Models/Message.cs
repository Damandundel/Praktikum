using System.ComponentModel.DataAnnotations;

namespace Praktikum.Data.Models;

/// <summary>Съобщение в разговор.</summary>
public class Message : BaseEntity
{
    public int ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    [Required]
    public string SenderId { get; set; } = string.Empty;

    public ApplicationUser Sender { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime SentOn { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }
}
