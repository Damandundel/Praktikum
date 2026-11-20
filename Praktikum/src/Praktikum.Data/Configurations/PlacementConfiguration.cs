using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class PlacementConfiguration : IEntityTypeConfiguration<Placement>
{
    public void Configure(EntityTypeBuilder<Placement> builder)
    {
        // От една кандидатура става най-много едно назначаване (връзка 1:1).
        builder.HasOne(p => p.Application)
            .WithOne(a => a.Placement)
            .HasForeignKey<Placement>(p => p.ApplicationId);

        // Към потребителя има две връзки (ученик и наставник), затова са описани изрично.
        builder.HasOne(p => p.Student).WithMany().HasForeignKey(p => p.StudentId);
        builder.HasOne(p => p.Mentor).WithMany().HasForeignKey(p => p.MentorId);

        builder.HasOne(p => p.InternshipPeriod)
            .WithMany(i => i.Placements)
            .HasForeignKey(p => p.InternshipPeriodId);

        // Един ученик има най-много едно неотказано назначаване за период.
        // 3 = PlacementStatus.Cancelled.
        builder.HasIndex(p => new { p.StudentId, p.InternshipPeriodId })
            .IsUnique()
            .HasFilter("[Status] <> 3");

        builder.Property(p => p.FinalGrade).HasPrecision(3, 2);

        builder.ToTable(t => t.HasCheckConstraint("CK_Placements_Dates", "[StartDate] <= [EndDate]"));
    }
}
