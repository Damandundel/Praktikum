using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.HasOne(a => a.Student)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.StudentId);

        builder.HasOne(a => a.InternshipPosition)
            .WithMany(p => p.Applications)
            .HasForeignKey(a => a.InternshipPositionId);

        // Ученик кандидатства за една позиция само веднъж.
        builder.HasIndex(a => new { a.StudentId, a.InternshipPositionId }).IsUnique();
    }
}
