using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class InternshipPositionConfiguration : IEntityTypeConfiguration<InternshipPosition>
{
    public void Configure(EntityTypeBuilder<InternshipPosition> builder)
    {
        builder.HasOne(p => p.Organization)
            .WithMany(o => o.Positions)
            .HasForeignKey(p => p.OrganizationId);

        builder.HasOne(p => p.InternshipPeriod)
            .WithMany(i => i.Positions)
            .HasForeignKey(p => p.InternshipPeriodId);

        builder.ToTable(t => t.HasCheckConstraint("CK_InternshipPositions_Capacity", "[Capacity] > 0"));
    }
}
