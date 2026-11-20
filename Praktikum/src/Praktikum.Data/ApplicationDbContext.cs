using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Praktikum.Data.Models;

namespace Praktikum.Data;

/// <summary>
/// Контекстът на базата данни. Наследява IdentityDbContext, за да добави таблиците
/// за потребители и роли (AspNetUsers, AspNetRoles...) към нашите.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<InternshipPeriod> InternshipPeriods => Set<InternshipPeriod>();

    public DbSet<InternshipPosition> InternshipPositions => Set<InternshipPosition>();

    public DbSet<Application> Applications => Set<Application>();

    public DbSet<Placement> Placements => Set<Placement>();

    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();

    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();

    public DbSet<EvaluationCriterion> EvaluationCriteria => Set<EvaluationCriterion>();

    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ProfileEntry> ProfileEntries => Set<ProfileEntry>();

    public DbSet<UserConnection> UserConnections => Set<UserConnection>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Зарежда всички IEntityTypeConfiguration<T> от този проект (папка Configurations).
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Изтриването е ограничено, за да не изчезват оценки и дневник каскадно.
        // Изключения: графикът и известията са "собственост" на родителя си.
        // Таблиците на Identity не се пипат (ползват собствените си правила).
        var restrictedForeignKeys = builder.Model.GetEntityTypes()
            .Where(e => e.ClrType.Namespace == typeof(ApplicationUser).Namespace
                        && e.ClrType != typeof(ScheduleSlot)
                        && e.ClrType != typeof(Notification)
                        && e.ClrType != typeof(ProfileEntry)
                        && e.ClrType != typeof(Message))
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => !fk.IsOwnership);

        foreach (var foreignKey in restrictedForeignKeys)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
