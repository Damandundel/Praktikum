using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class InternshipPeriodConfiguration : IEntityTypeConfiguration<InternshipPeriod>
{
    public void Configure(EntityTypeBuilder<InternshipPeriod> builder)
    {
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InternshipPeriods_Dates", "[StartDate] <= [EndDate]");
            t.HasCheckConstraint("CK_InternshipPeriods_Deadline", "[ApplicationDeadline] <= [StartDate]");
            t.HasCheckConstraint("CK_InternshipPeriods_RequiredHours", "[RequiredHours] > 0");
        });
    }
}
