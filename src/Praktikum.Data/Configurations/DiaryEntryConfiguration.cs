using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class DiaryEntryConfiguration : IEntityTypeConfiguration<DiaryEntry>
{
    public void Configure(EntityTypeBuilder<DiaryEntry> builder)
    {
        builder.HasOne(d => d.Placement)
            .WithMany(p => p.DiaryEntries)
            .HasForeignKey(d => d.PlacementId);

        builder.HasOne(d => d.ConfirmedBy).WithMany().HasForeignKey(d => d.ConfirmedById);

        // Ускорява справките по назначаване и дата.
        builder.HasIndex(d => new { d.PlacementId, d.Date });

        // 12 = GlobalConstants.Diary.MaxHoursPerDay.
        builder.ToTable(t => t.HasCheckConstraint("CK_DiaryEntries_Hours", "[Hours] > 0 AND [Hours] <= 12"));
    }
}
