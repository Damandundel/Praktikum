using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class EvaluationCriterionConfiguration : IEntityTypeConfiguration<EvaluationCriterion>
{
    public void Configure(EntityTypeBuilder<EvaluationCriterion> builder)
    {
        builder.HasIndex(c => c.Name).IsUnique();

        builder.ToTable(t => t.HasCheckConstraint("CK_EvaluationCriteria_MaxScore", "[MaxScore] > 0"));
    }
}
