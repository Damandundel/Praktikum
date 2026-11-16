using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Praktikum.Common;
using Praktikum.Data.Models;
using Praktikum.Data.Models.Enums;

namespace Praktikum.Data.Seeding;

/// <summary>
/// Допълнителни демо данни: още организации, позиции и акаунти, профили, връзки, задания с предадена
/// работа, приключили стажове с оценки и решения за наемане, и примерни разговори.
/// Всички данни са измислени. Оценките и точките са "случайни", но с фиксирано зърно (еднакви при всяко стартиране).
/// Сидърът е идемпотентен: проверява дали вече е изпълнен по един от новите акаунти.
/// </summary>
public class DemoNetworkSeeder : ISeeder
{
    public const string MarkerEmail = "ivan.petrov@praktikum.demo";

    private readonly Random _random = new(2027);

    public async Task SeedAsync(ApplicationDbContext db, IServiceProvider serviceProvider)
    {
        if (await db.Users.AnyAsync(u => u.Email == MarkerEmail))
        {
            return;
        }

        var baseOrganizations = await db.Organizations.OrderBy(o => o.Id).Take(3).ToListAsync();
        var demoStudent = await db.Users.FirstOrDefaultAsync(u => u.Email == DemoAccounts.StudentEmail);
        var demoMentor = await db.Users.FirstOrDefaultAsync(u => u.Email == DemoAccounts.MentorEmail);
        var demoEmployer = await db.Users.FirstOrDefaultAsync(u => u.Email == DemoAccounts.EmployerEmail);
        var demoCoordinator = await db.Users.FirstOrDefaultAsync(u => u.Email == DemoAccounts.CoordinatorEmail);
        var criteria = await db.EvaluationCriteria.ToListAsync();

        if (baseOrganizations.Count < 3 || demoStudent is null || demoMentor is null || demoEmployer is null)
        {
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var orgs = await SeedOrganizationsAsync(db, baseOrganizations);
        var people = await SeedPeopleAsync(userManager, orgs);
        var periods = await SeedPeriodsAsync(db, today);
        var positions = await SeedPositionsAsync(db, orgs, periods);

        ApplyProfiles(db, demoStudent, demoMentor, demoEmployer, demoCoordinator, people);
        await db.SaveChangesAsync();

        await SeedPastPlacementsAsync(db, positions, people, periods, criteria, demoMentor, demoEmployer);
        await SeedActivePlacementsAsync(db, positions, people, periods, demoStudent, demoMentor);
        await SeedApplicationsAsync(db, positions, people, demoStudent);
        await SeedConnectionsAsync(db, people, demoStudent, demoMentor, demoEmployer);
        await SeedConversationsAsync(db, people, demoStudent, demoMentor, demoEmployer, positions);
    }

    // ---------------------------------------------------------------- организации и хора

    private static async Task<List<Organization>> SeedOrganizationsAsync(ApplicationDbContext db, List<Organization> baseOrgs)
    {
        var extra = new List<Organization>
        {
            new()
            {
                Name = "Банка Инвест Демо",
                Description = "Демонстрационна финансова институция с екип за вътрешни системи и киберсигурност.",
                Address = "гр. София, бул. Примерен 80",
                ContactEmail = "careers@bankinvest.demo",
                ContactPhone = "+359 2 000 0004",
            },
            new()
            {
                Name = "Креатив Студио Демо",
                Description = "Демонстрационно студио за дизайн на интерфейси, брандинг и уеб презентации.",
                Address = "гр. Пловдив, ул. Примерна 12",
                ContactEmail = "hello@creativestudio.demo",
            },
        };

        db.Organizations.AddRange(extra);
        await db.SaveChangesAsync();
        return baseOrgs.Concat(extra).ToList();
    }

    private static async Task<Dictionary<string, ApplicationUser>> SeedPeopleAsync(
        UserManager<ApplicationUser> userManager, List<Organization> orgs)
    {
        // Ключ, роля, име, фамилия, организация (индекс или -1), клас.
        var definitions = new (string Key, string Role, string First, string Last, int Org, string? Class)[]
        {
            ("senior", GlobalConstants.Roles.Mentor, "Георги", "Тодоров", 0, null),
            ("mentor3", GlobalConstants.Roles.Mentor, "Силвия", "Маринова", 1, null),
            ("mentor4", GlobalConstants.Roles.Mentor, "Петя", "Христова", 2, null),
            ("mentor5", GlobalConstants.Roles.Mentor, "Калоян", "Василев", 3, null),
            ("mentor6", GlobalConstants.Roles.Mentor, "Десислава", "Йорданова", 4, null),
            ("employer2", GlobalConstants.Roles.Employer, "Анна", "Костова", 1, null),
            ("employer3", GlobalConstants.Roles.Employer, "Димитър", "Ненов", 2, null),
            ("employer4", GlobalConstants.Roles.Employer, "Радина", "Павлова", 3, null),
            ("employer5", GlobalConstants.Roles.Employer, "Стефан", "Ангелов", 4, null),
            ("ivan", GlobalConstants.Roles.Student, "Иван", "Петров", -1, "12А"),
            ("maria", GlobalConstants.Roles.Student, "Мария", "Георгиева", -1, "12Б"),
            ("nikolay", GlobalConstants.Roles.Student, "Николай", "Димитров", -1, "12А"),
            ("elitsa", GlobalConstants.Roles.Student, "Елица", "Стоянова", -1, "11А"),
            ("boris", GlobalConstants.Roles.Student, "Борис", "Колев", -1, "12Б"),
            ("yana", GlobalConstants.Roles.Student, "Яна", "Тодорова", -1, "11Б"),
            ("viktor", GlobalConstants.Roles.Student, "Виктор", "Асенов", -1, "11А"),
        };

        var emails = new Dictionary<string, string>
        {
            ["senior"] = "senior@praktikum.demo",
            ["mentor3"] = "mentor3@praktikum.demo",
            ["mentor4"] = "mentor4@praktikum.demo",
            ["mentor5"] = "mentor5@praktikum.demo",
            ["mentor6"] = "mentor6@praktikum.demo",
            ["employer2"] = "employer2@praktikum.demo",
            ["employer3"] = "employer3@praktikum.demo",
            ["employer4"] = "employer4@praktikum.demo",
            ["employer5"] = "employer5@praktikum.demo",
            ["ivan"] = MarkerEmail,
            ["maria"] = "maria.georgieva@praktikum.demo",
            ["nikolay"] = "nikolay.dimitrov@praktikum.demo",
            ["elitsa"] = "elitsa.stoyanova@praktikum.demo",
            ["boris"] = "boris.kolev@praktikum.demo",
            ["yana"] = "yana.todorova@praktikum.demo",
            ["viktor"] = "viktor.asenov@praktikum.demo",
        };

        var result = new Dictionary<string, ApplicationUser>();

        foreach (var d in definitions)
        {
            var user = new ApplicationUser
            {
                UserName = emails[d.Key],
                Email = emails[d.Key],
                EmailConfirmed = true,
                FirstName = d.First,
                LastName = d.Last,
                ClassName = d.Class,
                OrganizationId = d.Org >= 0 ? orgs[d.Org].Id : null,
            };

            var created = await userManager.CreateAsync(user, DemoAccounts.Password);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Потребителят '{user.Email}' не можа да бъде създаден: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, d.Role);
            result[d.Key] = user;
        }

        return result;
    }

    // ---------------------------------------------------------------- периоди и позиции

    private static async Task<Dictionary<string, InternshipPeriod>> SeedPeriodsAsync(ApplicationDbContext db, DateOnly today)
    {
        var existing = await db.InternshipPeriods.OrderBy(p => p.Id).ToListAsync();

        var periods = new Dictionary<string, InternshipPeriod>
        {
            ["past"] = new()
            {
                Name = "Миналогодишна практика (демо)",
                StartDate = today.AddDays(-400),
                EndDate = today.AddDays(-360),
                ApplicationDeadline = today.AddDays(-430),
                RequiredHours = 80,
            },
            ["current"] = existing[0],
            ["next"] = existing[1],
            ["autumn"] = new()
            {
                Name = "Есенна практика (демо)",
                StartDate = today.AddDays(40),
                EndDate = today.AddDays(75),
                ApplicationDeadline = today.AddDays(25),
                RequiredHours = 60,
            },
            ["summer"] = new()
            {
                Name = "Лятна практика (демо)",
                StartDate = today.AddDays(200),
                EndDate = today.AddDays(235),
                ApplicationDeadline = today.AddDays(150),
                RequiredHours = 120,
            },
        };

        db.InternshipPeriods.AddRange(periods.Values.Where(p => p.Id == 0));
        await db.SaveChangesAsync();
        return periods;
    }

    private static async Task<Dictionary<string, InternshipPosition>> SeedPositionsAsync(
        ApplicationDbContext db, List<Organization> orgs, Dictionary<string, InternshipPeriod> periods)
    {
        var existing = await db.InternshipPositions.OrderBy(p => p.Id).Take(4).ToListAsync();

        InternshipPosition P(string title, string description, int capacity, int org, string period) => new()
        {
            Title = title,
            Description = description,
            Capacity = capacity,
            Organization = orgs[org],
            InternshipPeriod = periods[period],
        };

        var positions = new Dictionary<string, InternshipPosition>
        {
            ["web"] = existing[0],
            ["catalog"] = existing[1],
            ["web-next"] = existing[2],
            ["energy-analyst"] = existing[3],

            // Минал период (приключили стажове).
            ["past-web"] = P("Junior .NET разработчик", "Разработка на API и работа с база данни по реален проект.", 2, 0, "past"),
            ["past-library"] = P("Помощник в каталогизирането", "Описване и проверка на библиографски записи.", 1, 2, "past"),
            ["past-energy"] = P("Асистент в анализа на данни", "Подготовка на таблици и графики за производство на енергия.", 1, 1, "past"),
            ["past-bank"] = P("Junior QA в банка", "Ръчно и автоматизирано тестване на вътрешна банкова система.", 1, 3, "past"),

            // Текущ период (заети позиции).
            ["data-viz"] = P("Визуализация на данни за слънчеви паркове", "Изграждане на табла с Power BI и Python по данни от соларни централи.", 1, 1, "current"),
            ["qa"] = P("Junior QA тестер", "Писане на тестови сценарии и откриване на грешки в банково приложение.", 1, 3, "current"),
            ["ux"] = P("UI/UX дизайнер стажант", "Дизайн на екрани във Figma и тестване на удобството с потребители.", 1, 4, "current"),

            // Отворени за кандидатстване.
            ["cyber"] = P("Стажант по киберсигурност", "Анализ на сигурността на уеб приложения (OWASP Top 10) под ръководство на експерт.", 2, 3, "autumn"),
            ["frontend"] = P("Junior front-end разработчик", "Създаване на адаптивни страници с HTML, CSS и JavaScript.", 2, 0, "autumn"),
            ["brand"] = P("Асистент по брандинг", "Изработка на визуална идентичност и материали за социални мрежи.", 1, 4, "autumn"),
            ["iot"] = P("IoT стажант в енергетиката", "Работа със сензори и микроконтролери за следене на соларни панели.", 1, 1, "autumn"),
            ["library-digital"] = P("Дигитализация на фондове", "Сканиране, обработка и качване на архивни документи в дигиталния каталог.", 2, 2, "autumn"),
            ["backend-summer"] = P("Back-end стажант (ASP.NET Core)", "Разработка на REST API, тестове и работа с MS SQL Server.", 2, 0, "summer"),
            ["mobile"] = P("Мобилна разработка с .NET MAUI", "Изграждане на мобилно приложение за обслужване на клиенти.", 1, 0, "summer"),
            ["data-bank"] = P("Анализатор на данни в банка", "SQL анализи и отчети за вътрешни екипи.", 1, 3, "summer"),
            ["motion"] = P("Motion дизайн стажант", "Кратки анимации за уеб и социални мрежи.", 1, 4, "summer"),
        };

        db.InternshipPositions.AddRange(positions.Values.Where(p => p.Id == 0));
        await db.SaveChangesAsync();
        return positions;
    }

    // ---------------------------------------------------------------- профили

    private void ApplyProfiles(
        ApplicationDbContext db,
        ApplicationUser demoStudent, ApplicationUser demoMentor, ApplicationUser demoEmployer,
        ApplicationUser? demoCoordinator, Dictionary<string, ApplicationUser> p)
    {
        void Profile(ApplicationUser u, string headline, string about, string school, string location)
        {
            u.Headline = headline;
            u.About = about;
            u.School = school;
            u.Location = location;
        }

        const string school = "ЧПГДН „СофтУни Будител“";

        Profile(demoStudent, "Ученик в 12 клас, специалност „Приложно програмиране“", "Търся първия си реален проект с .NET. Обичам да подреждам кода си и да уча от по-опитни колеги.", school, "София");
        Profile(demoMentor, "Наставник и старши разработчик в ТехноСофт", "Помагам на стажанти да направят първите си стъпки в професионалната разработка.", "ТехноСофт Демо ООД", "София");
        Profile(demoEmployer, "HR и набиране на таланти в ТехноСофт", "Търсим млади хора с желание за учене. Посрещаме стажанти всеки сезон.", "ТехноСофт Демо ООД", "София");
        if (demoCoordinator is not null)
        {
            Profile(demoCoordinator, "Координатор на практическото обучение", "Свързвам учениците с организации и следя изпълнението на практиките.", school, "София");
        }

        Profile(p["senior"], "Старши .NET разработчик", "10 години опит с .NET и облачни услуги. Обичам код ревюта и малките, ясни стъпки.", "ТехноСофт Демо ООД", "София");
        Profile(p["mentor3"], "Ръководител на екип „Данни“", "Работя с данни от соларни паркове и обучавам млади анализатори.", "Зелена Енергия Демо АД", "Пловдив");
        Profile(p["mentor4"], "Библиотекар-систематик", "Грижа се за дигиталния каталог на библиотеката.", "Градска библиотека Демо", "Варна");
        Profile(p["mentor5"], "QA инженер", "Тествам системи за плащания. Стажантите при мен научават как да мислят като тестери.", "Банка Инвест Демо", "София");
        Profile(p["mentor6"], "Арт директор", "Дизайн на продукти и бранд идентичност.", "Креатив Студио Демо", "Пловдив");
        Profile(p["employer2"], "Мениджър човешки ресурси", "Търсим стажанти в областта на данните и IoT.", "Зелена Енергия Демо АД", "Пловдив");
        Profile(p["employer3"], "Директор на библиотеката", "Отворени сме за млади хора, които обичат книгите и технологиите.", "Градска библиотека Демо", "Варна");
        Profile(p["employer4"], "Ръководител набиране", "Банка Инвест предлага стажове по тестване, киберсигурност и анализ на данни.", "Банка Инвест Демо", "София");
        Profile(p["employer5"], "Управляващ партньор", "Малко студио с голямо желание да обучава нови дизайнери.", "Креатив Студио Демо", "Пловдив");

        Profile(p["ivan"], "Ученик, запален по back-end разработка", "Правя малки API-та и обичам да чета чужд код. Приключих стаж в ТехноСофт.", school, "София");
        Profile(p["maria"], "Бъдещ библиотекар и уеб разработчик", "Интересувам се от каталогизиране, метаданни и търсене.", school, "Варна");
        Profile(p["nikolay"], "QA ентусиаст", "Обичам да намирам грешки преди потребителите.", school, "София");
        Profile(p["elitsa"], "Начинаещ UI/UX дизайнер", "Рисувам интерфейси във Figma и снимам фотографии.", school, "Пловдив");
        Profile(p["boris"], "Данни и графики", "Харесва ми да превръщам таблици в ясни картини.", school, "Пловдив");
        Profile(p["yana"], "Front-end разработчик", "HTML, CSS и малко JavaScript. Търся стаж през есента.", school, "София");
        Profile(p["viktor"], "Интересува се от киберсигурност", "Участвам в CTF състезания за начинаещи.", school, "София");

        void Entry(ApplicationUser u, ProfileEntryType type, string title, string? org, string? description, int fromYear, int? toYear)
        {
            db.ProfileEntries.Add(new ProfileEntry
            {
                User = u,
                Type = type,
                Title = title,
                Organization = org,
                Description = description,
                StartDate = new DateOnly(fromYear, 9, 15),
                EndDate = toYear is null ? null : new DateOnly(toYear.Value, 6, 30),
            });
        }

        Entry(demoStudent, ProfileEntryType.Education, "Приложно програмиране", school, "Профилирана подготовка: C#, бази данни, уеб.", 2023, null);
        Entry(demoStudent, ProfileEntryType.Achievement, "Второ място на училищен хакатон", school, "Екипен проект: приложение за обмен на учебни материали.", 2026, 2026);
        Entry(demoStudent, ProfileEntryType.Work, "Лятна работа като помощник в магазин", "Книжарница „Примерна“", "Работа с клиенти и каса.", 2025, 2025);

        Entry(p["ivan"], ProfileEntryType.Education, "Приложно програмиране", school, null, 2023, null);
        Entry(p["ivan"], ProfileEntryType.Achievement, "Първо място на олимпиада по информационни технологии", "Областен кръг", null, 2026, 2026);
        Entry(p["ivan"], ProfileEntryType.Work, "Фрийлансър - малки сайтове", null, "Три сайта за местни фирми.", 2025, null);
        Entry(p["maria"], ProfileEntryType.Education, "Приложно програмиране", school, null, 2023, null);
        Entry(p["maria"], ProfileEntryType.Achievement, "Награда за най-добър проект „Дигитален каталог“", school, null, 2026, 2026);
        Entry(p["nikolay"], ProfileEntryType.Education, "Приложно програмиране", school, null, 2023, null);
        Entry(p["nikolay"], ProfileEntryType.Achievement, "Сертификат „Основи на тестването“", "Онлайн курс", null, 2026, 2026);
        Entry(p["elitsa"], ProfileEntryType.Education, "Приложно програмиране", school, null, 2024, null);
        Entry(p["elitsa"], ProfileEntryType.Work, "Дизайн на плакати за училищни събития", school, "Над 20 плаката за училищни събития.", 2025, null);
        Entry(p["boris"], ProfileEntryType.Education, "Приложно програмиране", school, null, 2023, null);
        Entry(p["boris"], ProfileEntryType.Achievement, "Финалист в състезание по анализ на данни", "Национален кръг", null, 2026, 2026);
        Entry(p["yana"], ProfileEntryType.Education, "Приложно програмиране", school, null, 2024, null);
        Entry(p["yana"], ProfileEntryType.Achievement, "Лично портфолио с 5 проекта", "GitHub", null, 2026, null);
        Entry(p["viktor"], ProfileEntryType.Education, "Приложно програмиране", school, null, 2024, null);
        Entry(p["viktor"], ProfileEntryType.Achievement, "Топ 10% в CTF състезание за начинаещи", "CyberSchool", null, 2026, 2026);
    }

    // ---------------------------------------------------------------- приключили стажове

    private async Task SeedPastPlacementsAsync(
        ApplicationDbContext db,
        Dictionary<string, InternshipPosition> positions,
        Dictionary<string, ApplicationUser> people,
        Dictionary<string, InternshipPeriod> periods,
        List<EvaluationCriterion> criteria,
        ApplicationUser demoMentor,
        ApplicationUser demoEmployer)
    {
        var past = periods["past"];

        var items = new (string Student, string Position, ApplicationUser Mentor, HiringDecision Decision, string Summary)[]
        {
            ("ivan", "past-web", demoMentor, HiringDecision.NotDecided,
                "Иван разработи две крайни точки на REST API, написа unit тестове и участва в код ревюта. Работи самостоятелно, пита навреме и спазва срокове."),
            ("yana", "past-web", people["senior"], HiringDecision.Offered,
                "Яна изгради страница за управление на потребители и подобри адаптивността на приложението. Комуникира ясно и поема отговорност."),
            ("maria", "past-library", people["mentor4"], HiringDecision.NotDecided,
                "Мария каталогизира над 300 записа и предложи по-удобни етикети за търсене. Внимателна към детайлите и коректна."),
            ("boris", "past-energy", people["mentor3"], HiringDecision.Declined,
                "Борис подготви графики за производство на енергия. Показа добри основи, но има нужда от повече самостоятелност при анализа."),
            ("nikolay", "past-bank", people["mentor5"], HiringDecision.NotDecided,
                "Николай написа 40 тестови сценария и откри 12 дефекта. Отличен в екипна работа и документиране."),
        };

        foreach (var item in items)
        {
            var student = people[item.Student];
            var position = positions[item.Position];

            var application = new Application
            {
                Student = student,
                InternshipPosition = position,
                MotivationLetter = "Искам да натрупам реален опит и да науча от професионалисти.",
                Status = ApplicationStatus.Approved,
                AppliedOn = DateTime.UtcNow.AddDays(-440),
                ReviewedOn = DateTime.UtcNow.AddDays(-436),
                ReviewComment = "Одобрена.",
            };

            var grade = Math.Round((decimal)(4.5 + _random.NextDouble() * 1.5), 2);
            var placement = new Placement
            {
                Application = application,
                Student = student,
                Mentor = item.Mentor,
                InternshipPeriod = past,
                StartDate = past.StartDate,
                EndDate = past.EndDate,
                Status = PlacementStatus.Completed,
                CompletionSummary = item.Summary,
                FinalGrade = grade,
                CompletedOn = DateTime.UtcNow.AddDays(-358),
                HiringDecision = item.Decision,
                HiringDecidedOn = item.Decision == HiringDecision.NotDecided ? null : DateTime.UtcNow.AddDays(-350),
                HiringMessage = item.Decision switch
                {
                    HiringDecision.Offered => "Каним те да се присъединиш към екипа като junior след завършване!",
                    HiringDecision.Declined => "Благодарим ти за труда. Този път нямаме свободна позиция, но ще ти пишем при нужда.",
                    _ => null,
                },
            };

            foreach (var day in new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday })
            {
                placement.ScheduleSlots.Add(new ScheduleSlot { DayOfWeek = day, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(13, 0) });
            }

            db.Placements.Add(placement);
            await db.SaveChangesAsync();

            foreach (var criterion in criteria)
            {
                db.Evaluations.Add(new Evaluation
                {
                    PlacementId = placement.Id,
                    EvaluationCriterionId = criterion.Id,
                    EvaluatorId = item.Mentor.Id,
                    Score = _random.Next(7, GlobalConstants.Grading.MaxCriterionScore + 1),
                    EvaluatedOn = DateTime.UtcNow.AddDays(-359),
                });
            }

            // Три приключени задания с оценки (едно със закъснение).
            var titles = new[] { "Запознаване с проекта", "Основна задача", "Финална презентация" };
            for (var i = 0; i < titles.Length; i++)
            {
                var due = DateTime.UtcNow.AddDays(-390 + i * 14);
                var assignment = new Assignment
                {
                    PlacementId = placement.Id,
                    Title = titles[i],
                    Description = "Изпълнете задачата според обсъденото на срещата и предайте кратко описание.",
                    DueOn = due,
                    MaxPoints = 100,
                    CreatedById = item.Mentor.Id,
                    CreatedOn = due.AddDays(-10),
                };

                var points = _random.Next(70, 101);
                var late = i == 1 && _random.Next(0, 2) == 0;
                assignment.Submissions.Add(new Submission
                {
                    StudentId = student.Id,
                    Text = "Задачата е изпълнена. Описание на решението е приложено по-горе.",
                    SubmittedOn = late ? due.AddDays(1) : due.AddDays(-1),
                    IsLate = late,
                    Status = SubmissionStatus.Reviewed,
                    Points = points,
                    Feedback = points >= 90 ? "Отлична работа!" : "Добре, обърни внимание на детайлите.",
                    ReviewerId = item.Mentor.Id,
                    ReviewedOn = due.AddDays(2),
                });

                db.Assignments.Add(assignment);
            }

            await db.SaveChangesAsync();
        }
    }

    // ---------------------------------------------------------------- текущи стажове (заети позиции)

    private async Task SeedActivePlacementsAsync(
        ApplicationDbContext db,
        Dictionary<string, InternshipPosition> positions,
        Dictionary<string, ApplicationUser> people,
        Dictionary<string, InternshipPeriod> periods,
        ApplicationUser demoStudent,
        ApplicationUser demoMentor)
    {
        var current = periods["current"];

        // Назначението на демо ученика вече съществува - добавяме му задания.
        var demoPlacement = await db.Placements.FirstAsync(p => p.StudentId == demoStudent.Id && p.Status == PlacementStatus.Active);
        await AddAssignmentsAsync(db, demoPlacement, demoMentor, demoStudent, rich: true);

        var items = new (string Student, string Position, ApplicationUser Mentor)[]
        {
            ("ivan", "web", people["senior"]),
            ("maria", "catalog", people["mentor4"]),
            ("boris", "data-viz", people["mentor3"]),
            ("nikolay", "qa", people["mentor5"]),
            ("elitsa", "ux", people["mentor6"]),
        };

        foreach (var item in items)
        {
            var student = people[item.Student];
            var placement = new Placement
            {
                Application = new Application
                {
                    Student = student,
                    InternshipPosition = positions[item.Position],
                    MotivationLetter = "Искам да продължа развитието си в тази посока.",
                    Status = ApplicationStatus.Approved,
                    AppliedOn = DateTime.UtcNow.AddDays(-42),
                    ReviewedOn = DateTime.UtcNow.AddDays(-39),
                    ReviewComment = "Одобрена.",
                },
                Student = student,
                Mentor = item.Mentor,
                InternshipPeriod = current,
                StartDate = current.StartDate,
                EndDate = current.EndDate,
                Status = PlacementStatus.Active,
            };

            foreach (var day in new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday })
            {
                placement.ScheduleSlots.Add(new ScheduleSlot { DayOfWeek = day, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(14, 0) });
            }

            db.Placements.Add(placement);
            await db.SaveChangesAsync();
            await AddAssignmentsAsync(db, placement, item.Mentor, student, rich: false);
        }
    }

    private async Task AddAssignmentsAsync(
        ApplicationDbContext db, Placement placement, ApplicationUser mentor, ApplicationUser student, bool rich)
    {
        var now = DateTime.UtcNow;

        // 1) оценено в срок
        var first = new Assignment
        {
            PlacementId = placement.Id,
            Title = "Настройка на работната среда",
            Description = "Инсталирайте нужните инструменти, клонирайте проекта и стартирайте го локално. Опишете какви трудности сте срещнали.",
            DueOn = now.AddDays(-14),
            MaxPoints = 100,
            CreatedById = mentor.Id,
            CreatedOn = now.AddDays(-20),
        };
        var firstPoints = rich ? 92 : _random.Next(75, 101);
        first.Submissions.Add(new Submission
        {
            StudentId = student.Id,
            Text = "Средата е настроена. Срещнах проблем с версията на SDK и го реших с обновяване.",
            SubmittedOn = now.AddDays(-15),
            Status = SubmissionStatus.Reviewed,
            Points = firstPoints,
            Feedback = "Добра и точна работа, браво.",
            ReviewerId = mentor.Id,
            ReviewedOn = now.AddDays(-13),
        });

        // 2) оценено със закъснение (отстъпка)
        var second = new Assignment
        {
            PlacementId = placement.Id,
            Title = "Първа малка функционалност",
            Description = "Добавете страница със списък и филтриране по текст. Опишете как проверихте, че работи.",
            DueOn = now.AddDays(-8),
            MaxPoints = 100,
            CreatedById = mentor.Id,
            CreatedOn = now.AddDays(-14),
        };
        second.Submissions.Add(new Submission
        {
            StudentId = student.Id,
            Text = "Функционалността е готова, но предадох след срока заради контролни.",
            SubmittedOn = now.AddDays(-6),
            IsLate = true,
            Status = SubmissionStatus.Reviewed,
            Points = rich ? 88 : _random.Next(70, 96),
            Feedback = "Решението е добро. Следващия път планирай времето по-рано.",
            ReviewerId = mentor.Id,
            ReviewedOn = now.AddDays(-4),
        });

        // 3) предадено, чака преглед
        var third = new Assignment
        {
            PlacementId = placement.Id,
            Title = rich ? "REST API за каталог" : "Работа по текущата задача",
            Description = "Реализирайте крайни точки за четене и добавяне. Добавете описание на заявките и примерни отговори.",
            DueOn = now.AddDays(2),
            MaxPoints = 100,
            CreatedById = mentor.Id,
            CreatedOn = now.AddDays(-5),
        };
        third.Submissions.Add(new Submission
        {
            StudentId = student.Id,
            Text = "Реализирах GET и POST. Тествах с Postman. Прикачвам описание на заявките в текста.",
            SubmittedOn = now.AddHours(-6),
            Status = SubmissionStatus.Submitted,
        });

        db.Assignments.AddRange(first, second, third);

        // 4) предстои (непредадено)
        db.Assignments.Add(new Assignment
        {
            PlacementId = placement.Id,
            Title = "Unit тестове",
            Description = "Напишете поне пет unit теста за бизнес логиката и споделете резултата от изпълнението им.",
            DueOn = now.AddDays(9),
            MaxPoints = 100,
            CreatedById = mentor.Id,
            CreatedOn = now.AddDays(-2),
        });

        if (rich)
        {
            // 5) върнато за доработка
            var revision = new Assignment
            {
                PlacementId = placement.Id,
                Title = "Документация на проекта",
                Description = "Опишете архитектурата и стъпките за стартиране в README файл.",
                DueOn = now.AddDays(5),
                MaxPoints = 50,
                CreatedById = mentor.Id,
                CreatedOn = now.AddDays(-7),
            };
            revision.Submissions.Add(new Submission
            {
                StudentId = student.Id,
                Text = "Първа версия на документацията.",
                SubmittedOn = now.AddDays(-2),
                Status = SubmissionStatus.NeedsRevision,
                Feedback = "Добави диаграма на слоевете и стъпки за стартиране на базата.",
                ReviewerId = mentor.Id,
                ReviewedOn = now.AddDays(-1),
            });
            db.Assignments.Add(revision);

            // 6) просрочено, непредадено
            db.Assignments.Add(new Assignment
            {
                PlacementId = placement.Id,
                Title = "Код ревю на чужд проект",
                Description = "Прегледайте предоставения проект и опишете три подобрения.",
                DueOn = now.AddDays(-1),
                MaxPoints = 60,
                CreatedById = mentor.Id,
                CreatedOn = now.AddDays(-9),
            });
        }

        await db.SaveChangesAsync();
    }

    // ---------------------------------------------------------------- кандидатури, връзки, чат

    private static async Task SeedApplicationsAsync(
        ApplicationDbContext db,
        Dictionary<string, InternshipPosition> positions,
        Dictionary<string, ApplicationUser> people,
        ApplicationUser demoStudent)
    {
        void Apply(ApplicationUser student, string position, ApplicationStatus status, string letter)
        {
            db.Applications.Add(new Application
            {
                Student = student,
                InternshipPosition = positions[position],
                MotivationLetter = letter,
                Status = status,
                AppliedOn = DateTime.UtcNow.AddDays(-3),
                ReviewedOn = status == ApplicationStatus.Pending ? null : DateTime.UtcNow.AddDays(-1),
                ReviewComment = status == ApplicationStatus.Rejected ? "Този път избрахме други кандидати." : null,
            });
        }

        Apply(demoStudent, "frontend", ApplicationStatus.Pending, "Харесвам front-end и вече имам няколко малки проекта с HTML и CSS.");
        Apply(demoStudent, "cyber", ApplicationStatus.Rejected, "Интересувам се от сигурността на уеб приложения.");
        Apply(people["yana"], "frontend", ApplicationStatus.Pending, "Търся есенен стаж за front-end разработка.");
        Apply(people["viktor"], "cyber", ApplicationStatus.Pending, "Участвам в CTF състезания и искам да науча повече.");
        Apply(people["elitsa"], "brand", ApplicationStatus.Pending, "Обичам визуалния дизайн и бих искала да работя по брандинг.");
        Apply(people["maria"], "library-digital", ApplicationStatus.Pending, "Интересувам се от дигитализацията на архивни документи.");
        Apply(people["ivan"], "backend-summer", ApplicationStatus.Pending, "Искам да продължа в ASP.NET Core през лятото.");
        Apply(people["boris"], "iot", ApplicationStatus.Pending, "Работил съм с Arduino и данни от сензори.");

        await db.SaveChangesAsync();
    }

    private static async Task SeedConnectionsAsync(
        ApplicationDbContext db,
        Dictionary<string, ApplicationUser> people,
        ApplicationUser demoStudent, ApplicationUser demoMentor, ApplicationUser demoEmployer)
    {
        void Link(ApplicationUser a, ApplicationUser b, ConnectionStatus status = ConnectionStatus.Accepted)
            => db.UserConnections.Add(new UserConnection
            {
                Requester = a,
                Addressee = b,
                Status = status,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
            });

        Link(demoStudent, people["ivan"]);
        Link(demoStudent, people["maria"]);
        Link(demoStudent, demoMentor);
        Link(demoStudent, demoEmployer);
        Link(people["yana"], demoStudent, ConnectionStatus.Pending);
        Link(demoStudent, people["elitsa"], ConnectionStatus.Pending);
        Link(people["viktor"], demoStudent, ConnectionStatus.Pending);

        Link(demoEmployer, demoMentor);
        Link(demoEmployer, people["ivan"]);
        Link(demoEmployer, people["yana"]);
        Link(people["employer4"], people["nikolay"]);
        Link(people["employer2"], people["boris"]);
        Link(people["ivan"], people["maria"]);
        Link(people["ivan"], people["nikolay"]);
        Link(people["elitsa"], people["yana"]);

        await db.SaveChangesAsync();
    }

    private static async Task SeedConversationsAsync(
        ApplicationDbContext db,
        Dictionary<string, ApplicationUser> people,
        ApplicationUser demoStudent, ApplicationUser demoMentor, ApplicationUser demoEmployer,
        Dictionary<string, InternshipPosition> positions)
    {
        var now = DateTime.UtcNow;

        async Task Chat(
            ApplicationUser a, ApplicationUser b, ConversationTopic topic, string subject, string key,
            params (bool FromA, string Text, int MinutesAgo, bool Read)[] lines)
        {
            var conversation = new Conversation
            {
                UserA = a,
                UserB = b,
                Topic = topic,
                Subject = subject,
                ContextKey = key,
                CreatedOn = now.AddMinutes(-lines[0].MinutesAgo),
                LastMessageOn = now.AddMinutes(-lines[^1].MinutesAgo),
            };

            foreach (var line in lines)
            {
                conversation.Messages.Add(new Message
                {
                    Sender = line.FromA ? a : b,
                    Text = line.Text,
                    SentOn = now.AddMinutes(-line.MinutesAgo),
                    IsRead = line.Read,
                });
            }

            db.Conversations.Add(conversation);
            await db.SaveChangesAsync();
        }

        await Chat(demoStudent, demoEmployer, ConversationTopic.PositionInquiry,
            $"Попитване: {positions["frontend"].Title}", $"position:{positions["frontend"].Id}",
            (true, "Здравейте! Има ли още свободни места за front-end позицията и какви технологии ползвате?", 1500, true),
            (false, "Здравей! Да, остават 2 места. Работим с HTML, CSS, JavaScript и малко Razor. Прати ни мотивационно писмо.", 1440, true),
            (true, "Благодаря, вече кандидатствах. Може ли да ми кажете кога ще има отговор?", 1400, true),
            (false, "До края на следващата седмица. Успех!", 120, false));

        var assignment = await db.Assignments.FirstAsync(a => a.Title == "REST API за каталог");
        await Chat(demoStudent, demoMentor, ConversationTopic.AssignmentComment,
            $"Задание: {assignment.Title}", $"assignment:{assignment.Id}",
            (true, "Здравейте, не съм сигурен дали отговорите на POST заявката трябва да връщат 201 или 200.", 600, true),
            (false, "Върни 201 Created с адреса на новия ресурс в заглавката Location. Остави ми коментар, когато го пробваш.", 560, true),
            (true, "Готово, добавих го и обнових предаването.", 300, false));

        var placement = await db.Placements.Include(p => p.Student).Include(p => p.Mentor)
            .FirstAsync(p => p.Student.Email == DemoNetworkSeeder.MarkerEmail && p.Status == PlacementStatus.Completed);
        await Chat(people["ivan"], placement.Mentor, ConversationTopic.InternshipFeedback,
            "Обратна връзка: Junior .NET разработчик", $"placement:{placement.Id}",
            (true, "Благодаря за стажа! Какво да подобря, ако кандидатствам отново?", 5000, true),
            (false, "Работи повече върху писането на тестове и ранното споделяне на въпроси. Иначе беше отличен.", 4990, true));

        await Chat(people["viktor"], demoEmployer, ConversationTopic.General, "Лично съобщение", "direct",
            (true, "Здравейте! Мога ли да разгледам вашия профил и възможностите за стаж?", 90, false));
    }
}
