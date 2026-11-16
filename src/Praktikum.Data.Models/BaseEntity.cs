using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktikum.Common;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Models;

/// <summary>Базов клас с първичен ключ за всички домейн модели (освен потребителя).</summary>
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
}
