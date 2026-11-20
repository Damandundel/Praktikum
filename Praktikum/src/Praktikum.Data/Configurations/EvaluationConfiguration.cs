using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.HasOne(e => e.Placement)
            .WithMany(p => p.Evaluations)
            .HasForeignKey(e => e.PlacementId);

        builder.HasOne(e => e.EvaluationCriterion)
            .WithMany(c => c.Evaluations)
            .HasForeignKey(e => e.EvaluationCriterionId);

        builder.HasOne(e => e.Evaluator).WithMany().HasForeignKey(e => e.EvaluatorId);

        // Едно назначаване се оценява веднъж по всеки критерий.
        builder.HasIndex(e => new { e.PlacementId, e.EvaluationCriterionId }).IsUnique();

        builder.ToTable(t => t.HasCheckConstraint("CK_Evaluations_Score", "[Score] >= 0"));
    }
}
