using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class ScheduleSlotConfiguration : IEntityTypeConfiguration<ScheduleSlot>
{
    public void Configure(EntityTypeBuilder<ScheduleSlot> builder)
    {
        // При изтриване на назначаване се трие и графикът му.
        builder.HasOne(s => s.Placement)
            .WithMany(p => p.ScheduleSlots)
            .HasForeignKey(s => s.PlacementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("CK_ScheduleSlots_Times", "[StartTime] < [EndTime]"));
    }
}
