using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Praktikum.Data.Models;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Seeding;

/// <summary>
/// Попълва базата с демо данни, за да работи демото веднага след деплой.
/// Всички данни са измислени. Датите са спрямо деня на първото стартиране,
/// така че "текущият" период винаги е в ход и в дневника има скорошни записи.
/// Сидърът е идемпотентен: ако в базата вече има организации, не прави нищо.
/// </summary>
public class DemoDataSeeder : ISeeder
{
    private static readonly string[] Activities =
    {
        "Запознаване с екипа, работната среда и вътрешните правила.",
        "Настройване на работна станция и инструменти за разработка.",
        "Разработване на малка функционалност по зададена задача.",
        "Преглед на код (code review) заедно с наставника.",
        "Писане на тестове и поправяне на открити грешки.",
        "Подготовка на документация за извършената работа.",
    };

    public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
    {
        if (await dbContext.Organizations.AnyAsync())
        {
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var organizations = await SeedOrganizationsAsync(dbContext);
        var users = await SeedUsersAsync(userManager, organizations[0]);
        var criteria = await SeedCriteriaAsync(dbContext);
        var positions = await SeedPeriodsAndPositionsAsync(dbContext, organizations, today);
        var placement = await SeedApplicationsAndPlacementAsync(dbContext, users, positions);
        await SeedDiaryAsync(dbContext, placement, users[DemoAccounts.MentorEmail], today);
        await SeedNotificationsAsync(dbContext, users);
    }

    private static async Task<List<Organization>> SeedOrganizationsAsync(ApplicationDbContext db)
    {
        var organizations = new List<Organization>
        {
            new()
            {
                Name = "ТехноСофт Демо ООД",
                Description = "Демонстрационна софтуерна фирма за уеб и мобилни приложения.",
                Address = "гр. София, ул. Примерна 1",
                ContactEmail = "contact@technosoft.demo",
                ContactPhone = "+359 2 000 0001",
            },
            new()
            {
                Name = "Зелена Енергия Демо АД",
                Description = "Демонстрационна компания за възобновяема енергия.",
                Address = "гр. Пловдив, бул. Примерен 25",
                ContactEmail = "office@greenenergy.demo",
                ContactPhone = "+359 32 000 002",
            },
            new()
            {
                Name = "Градска библиотека Демо",
                Description = "Демонстрационна библиотека с дигитален каталог.",
                Address = "гр. Варна, пл. Примерен 3",
                ContactEmail = "info@library.demo",
            },
        };

        db.Organizations.AddRange(organizations);
        await db.SaveChangesAsync();
        return organizations;
    }

    private static async Task<Dictionary<string, ApplicationUser>> SeedUsersAsync(
        UserManager<ApplicationUser> userManager, Organization employerOrganization)
    {
        var users = new Dictionary<string, ApplicationUser>();

        foreach (var (email, role, firstName, lastName) in DemoAccounts.All)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
            };

            // Работодателят и наставникът са служители на първата организация.
            if (role is Common.GlobalConstants.Roles.Employer or Common.GlobalConstants.Roles.Mentor)
            {
                user.OrganizationId = employerOrganization.Id;
            }

            if (role == Common.GlobalConstants.Roles.Student)
            {
                user.ClassName = "12А";
            }

            var result = await userManager.CreateAsync(user, DemoAccounts.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Потребителят '{email}' не можа да бъде създаден: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, role);
            users[email] = user;
        }

        return users;
    }

    private static async Task<List<EvaluationCriterion>> SeedCriteriaAsync(ApplicationDbContext db)
    {
        var criteria = new List<EvaluationCriterion>
        {
            new() { Name = "Отговорност", Description = "Изпълнява задачите навреме и коректно." },
            new() { Name = "Спазване на графика", Description = "Идва навреме и спазва уговорените часове." },
            new() { Name = "Технически умения", Description = "Прилага наученото в практически задачи." },
            new() { Name = "Работа в екип", Description = "Сътрудничи си с колегите и наставника." },
            new() { Name = "Комуникация", Description = "Изразява се ясно и задава въпроси при нужда." },
        };

        db.EvaluationCriteria.AddRange(criteria);
        await db.SaveChangesAsync();
        return criteria;
    }

    /// <summary>
    /// Два периода: текущ (в ход, с назначение) и следващ (отворен за кандидатстване).
    /// Връща позициите в ред: [0] и [1] са в текущия период, [2] и [3] в следващия.
    /// </summary>
    private static async Task<List<InternshipPosition>> SeedPeriodsAndPositionsAsync(
        ApplicationDbContext db, List<Organization> organizations, DateOnly today)
    {
        var current = new InternshipPeriod
        {
            Name = "Текуща практика (демо)",
            StartDate = today.AddDays(-21),
            EndDate = today.AddDays(21),
            ApplicationDeadline = today.AddDays(-35),
            RequiredHours = 80,
        };

        var next = new InternshipPeriod
        {
            Name = "Следваща практика (демо)",
            StartDate = today.AddDays(60),
            EndDate = today.AddDays(90),
            ApplicationDeadline = today.AddDays(45),
            RequiredHours = 80,
        };

        var positions = new List<InternshipPosition>
        {
            new()
            {
                Title = "Junior уеб разработчик",
                Description = "Работа по уеб приложение с ASP.NET Core под ръководството на наставник.",
                Capacity = 2,
                Organization = organizations[0],
                InternshipPeriod = current,
            },
            new()
            {
                Title = "Асистент в дигитален каталог",
                Description = "Въвеждане и проверка на данни в дигиталния каталог на библиотеката.",
                Capacity = 1,
                Organization = organizations[2],
                InternshipPeriod = current,
            },
            new()
            {
                Title = "Junior уеб разработчик (следващ период)",
                Description = "Разработка на нови функционалности и тестване на уеб приложение.",
                Capacity = 2,
                Organization = organizations[0],
                InternshipPeriod = next,
            },
            new()
            {
                Title = "Анализатор на енергийни данни",
                Description = "Събиране и визуализиране на данни за производство на енергия.",
                Capacity = 1,
                Organization = organizations[1],
                InternshipPeriod = next,
            },
        };

        db.InternshipPositions.AddRange(positions);
        await db.SaveChangesAsync();
        return positions;
    }

    /// <summary>
    /// Ученикът има одобрена кандидатура с назначение в текущия период
    /// и чакаща кандидатура за следващия (за демонстрация на одобряване от координатор).
    /// </summary>
    private static async Task<Placement> SeedApplicationsAndPlacementAsync(
        ApplicationDbContext db, Dictionary<string, ApplicationUser> users, List<InternshipPosition> positions)
    {
        var student = users[DemoAccounts.StudentEmail];
        var mentor = users[DemoAccounts.MentorEmail];
        var currentPeriod = positions[0].InternshipPeriod;

        var approved = new Application
        {
            Student = student,
            InternshipPosition = positions[0],
            MotivationLetter = "Искам да приложа наученото в училище по реален проект.",
            Status = ApplicationStatus.Approved,
            AppliedOn = DateTime.UtcNow.AddDays(-40),
            ReviewedOn = DateTime.UtcNow.AddDays(-38),
            ReviewComment = "Одобрена. Добре дошъл в екипа!",
        };

        var pending = new Application
        {
            Student = student,
            InternshipPosition = positions[3],
            MotivationLetter = "Интересувам се от анализ на данни и възобновяема енергия.",
            Status = ApplicationStatus.Pending,
            AppliedOn = DateTime.UtcNow.AddDays(-2),
        };

        var placement = new Placement
        {
            Application = approved,
            Student = student,
            Mentor = mentor,
            InternshipPeriod = currentPeriod,
            StartDate = currentPeriod.StartDate,
            EndDate = currentPeriod.EndDate,
            Status = PlacementStatus.Active,
        };

        // Работни слотове: понеделник, сряда и петък от 09:00 до 13:00 (по 4 часа).
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday })
        {
            placement.ScheduleSlots.Add(new ScheduleSlot
            {
                DayOfWeek = day,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(13, 0),
            });
        }

        db.Applications.Add(pending);
        db.Placements.Add(placement);
        await db.SaveChangesAsync();
        return placement;
    }

    /// <summary>
    /// Записи за всеки работен ден от графика до вчера. Всички са потвърдени,
    /// освен последните два, които чакат наставника (за демонстрация на потвърждаване).
    /// </summary>
    private static async Task SeedDiaryAsync(
        ApplicationDbContext db, Placement placement, ApplicationUser mentor, DateOnly today)
    {
        var workDays = placement.ScheduleSlots.Select(s => s.DayOfWeek).ToHashSet();
        var dates = new List<DateOnly>();

        for (var date = placement.StartDate; date < today; date = date.AddDays(1))
        {
            if (workDays.Contains(date.DayOfWeek))
            {
                dates.Add(date);
            }
        }

        for (var i = 0; i < dates.Count; i++)
        {
            var isPending = i >= dates.Count - 2;
            var entry = new DiaryEntry
            {
                PlacementId = placement.Id,
                Date = dates[i],
                Hours = 4,
                Activity = Activities[i % Activities.Length],
                Status = isPending ? DiaryEntryStatus.Pending : DiaryEntryStatus.Confirmed,
            };

            if (!isPending)
            {
                entry.ConfirmedById = mentor.Id;
                entry.ConfirmedOn = DateTime.UtcNow.AddDays(-1);
            }

            db.DiaryEntries.Add(entry);
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedNotificationsAsync(
        ApplicationDbContext db, Dictionary<string, ApplicationUser> users)
    {
        db.Notifications.AddRange(
            new Notification
            {
                UserId = users[DemoAccounts.MentorEmail].Id,
                Title = "Непотвърдени записи в дневника",
                Message = "Ученикът има записи в дневника, които чакат вашето потвърждение.",
                Type = NotificationType.DiaryPendingConfirmation,
            },
            new Notification
            {
                UserId = users[DemoAccounts.StudentEmail].Id,
                Title = "Срок за кандидатстване",
                Message = "Срокът за кандидатстване за следващата практика наближава.",
                Type = NotificationType.ApplicationDeadline,
            });

        await db.SaveChangesAsync();
    }
}
