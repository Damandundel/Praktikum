using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praktikum.Data.Models;

namespace Praktikum.Data.Configurations;

public class ProfileEntryConfiguration : IEntityTypeConfiguration<ProfileEntry>
{
    public void Configure(EntityTypeBuilder<ProfileEntry> builder)
    {
        builder.HasOne(e => e.User)
            .WithMany(u => u.ProfileEntries)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId);
    }
}

public class UserConnectionConfiguration : IEntityTypeConfiguration<UserConnection>
{
    public void Configure(EntityTypeBuilder<UserConnection> builder)
    {
        builder.HasOne(c => c.Requester).WithMany().HasForeignKey(c => c.RequesterId);
        builder.HasOne(c => c.Addressee).WithMany().HasForeignKey(c => c.AddresseeId);

        // Една заявка от един към друг потребител; сам към себе си не може.
        builder.HasIndex(c => new { c.RequesterId, c.AddresseeId }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("CK_UserConnections_NotSelf", "[RequesterId] <> [AddresseeId]"));
    }
}

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.HasOne(a => a.Placement)
            .WithMany(p => p.Assignments)
            .HasForeignKey(a => a.PlacementId);

        builder.HasOne(a => a.CreatedBy).WithMany().HasForeignKey(a => a.CreatedById);

        builder.HasIndex(a => a.PlacementId);
        builder.ToTable(t => t.HasCheckConstraint("CK_Assignments_MaxPoints", "[MaxPoints] > 0"));
    }
}

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId);

        builder.HasOne(s => s.Student).WithMany().HasForeignKey(s => s.StudentId);
        builder.HasOne(s => s.Reviewer).WithMany().HasForeignKey(s => s.ReviewerId);

        // Едно предаване на задание от ученик (повторно предаване обновява същия запис).
        builder.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
    }
}

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasOne(c => c.UserA).WithMany().HasForeignKey(c => c.UserAId);
        builder.HasOne(c => c.UserB).WithMany().HasForeignKey(c => c.UserBId);

        builder.HasIndex(c => new { c.UserAId, c.UserBId });
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId);

        builder.HasIndex(m => new { m.ConversationId, m.Id });
    }
}
