using System.ComponentModel.DataAnnotations;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Разговор между два акаунта по определена тема (попитване, задание, обратна връзка).</summary>
public class Conversation : BaseEntity
{
    [Required]
    public string UserAId { get; set; } = string.Empty;

    public ApplicationUser UserA { get; set; } = null!;

    [Required]
    public string UserBId { get; set; } = string.Empty;

    public ApplicationUser UserB { get; set; } = null!;

    [Required]
    [MaxLength(GlobalConstants.Lengths.Title)]
    public string Subject { get; set; } = string.Empty;

    public ConversationTopic Topic { get; set; } = ConversationTopic.General;

    /// <summary>Ключ на контекста (например "position:5"), за да не се дублират разговорите по една тема.</summary>
    [MaxLength(60)]
    public string? ContextKey { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime LastMessageOn { get; set; } = DateTime.UtcNow;

    public ICollection<Message> Messages { get; set; } = new HashSet<Message>();
}
