using System.ComponentModel.DataAnnotations;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Връзка (connection) между двама потребители - заявка и нейното приемане.</summary>
public class UserConnection : BaseEntity
{
    [Required]
    public string RequesterId { get; set; } = string.Empty;

    public ApplicationUser Requester { get; set; } = null!;

    [Required]
    public string AddresseeId { get; set; } = string.Empty;

    public ApplicationUser Addressee { get; set; } = null!;

    public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
