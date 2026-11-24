# ER диаграма – Praktikum

```mermaid
erDiagram
    ApplicationUser {
        string Id PK
        string FirstName
        string LastName
        string ClassName "само за ученик"
        string Headline "профил: кратко представяне"
        string About "профил: за мен"
        string School "профил: училище"
        string Location "профил: град"
        int OrganizationId FK "работодател и наставник"
        bool IsActive
        datetime CreatedOn
    }
    Organization {
        int Id PK
        string Name
        string Description
        string Address
        string ContactEmail
        string ContactPhone
        bool IsActive
    }
    InternshipPeriod {
        int Id PK
        string Name
        date StartDate
        date EndDate
        date ApplicationDeadline
        int RequiredHours
        bool IsActive
    }
    InternshipPosition {
        int Id PK
        string Title
        string Description
        int Capacity
        bool IsOpen
        int OrganizationId FK
        int InternshipPeriodId FK
    }
    Application {
        int Id PK
        string StudentId FK
        int InternshipPositionId FK
        string MotivationLetter
        int Status
        datetime AppliedOn
        datetime ReviewedOn
        string ReviewComment
    }
    Placement {
        int Id PK
        int ApplicationId FK "уникален"
        string StudentId FK
        int InternshipPeriodId FK
        string MentorId FK
        date StartDate
        date EndDate
        int Status
        string CompletionSummary "описание от наставника"
        decimal FinalGrade "оценка 2-6"
        datetime CompletedOn
        int HiringDecision "решение на работодателя"
        string HiringMessage
        datetime HiringDecidedOn
    }
    ScheduleSlot {
        int Id PK
        int PlacementId FK
        int DayOfWeek
        time StartTime
        time EndTime
    }
    DiaryEntry {
        int Id PK
        int PlacementId FK
        date Date
        decimal Hours
        string Activity
        int Status
        string ConfirmedById FK
        datetime ConfirmedOn
        string MentorComment
    }
    EvaluationCriterion {
        int Id PK
        string Name
        string Description
        int MaxScore
        bool IsActive
    }
    Evaluation {
        int Id PK
        int PlacementId FK
        int EvaluationCriterionId FK
        string EvaluatorId FK
        int Score
        string Comment
        datetime EvaluatedOn
    }
    Notification {
        int Id PK
        string UserId FK
        string Title
        string Message
        int Type
        bool IsRead
        datetime CreatedOn
    }
    ProfileEntry {
        int Id PK
        string UserId FK
        int Type "образование, стаж, работа, постижение"
        string Title
        string Organization
        string Description
        date StartDate
        date EndDate
    }
    UserConnection {
        int Id PK
        string RequesterId FK
        string AddresseeId FK
        int Status "чака или приета"
        datetime CreatedOn
    }
    Assignment {
        int Id PK
        int PlacementId FK
        string Title
        string Description
        datetime DueOn "срок (UTC)"
        int MaxPoints
        string CreatedById FK
        datetime CreatedOn
    }
    Submission {
        int Id PK
        int AssignmentId FK
        string StudentId FK
        string Text
        string FileName
        string StoredFileName "случайно име извън wwwroot"
        datetime SubmittedOn
        bool IsLate "отстъпка 20%"
        int Status
        int Points
        string Feedback
        string ReviewerId FK
        datetime ReviewedOn
    }
    Conversation {
        int Id PK
        string UserAId FK
        string UserBId FK
        string Subject
        int Topic "позиция, задание, обратна връзка, общ"
        string ContextKey
        datetime CreatedOn
        datetime LastMessageOn
    }
    Message {
        int Id PK
        int ConversationId FK
        string SenderId FK
        string Text
        datetime SentOn
        bool IsRead
    }

    ApplicationUser ||--o{ ProfileEntry : "профил (CV)"
    ApplicationUser ||--o{ UserConnection : "изпраща заявка"
    ApplicationUser ||--o{ UserConnection : "получава заявка"
    Placement ||--o{ Assignment : "задания"
    Assignment ||--o{ Submission : "предавания"
    ApplicationUser ||--o{ Submission : "ученик предава"
    ApplicationUser ||--o{ Conversation : "участник"
    Conversation ||--o{ Message : "съобщения"
    ApplicationUser ||--o{ Message : "изпраща"
    Organization ||--o{ ApplicationUser : "служители"
    Organization ||--o{ InternshipPosition : "предлага"
    InternshipPeriod ||--o{ InternshipPosition : "включва"
    InternshipPeriod ||--o{ Placement : "в период"
    InternshipPosition ||--o{ Application : "получава"
    ApplicationUser ||--o{ Application : "ученик кандидатства"
    Application ||--o| Placement : "одобрена става"
    ApplicationUser ||--o{ Placement : "ученик"
    ApplicationUser ||--o{ Placement : "наставник"
    Placement ||--o{ ScheduleSlot : "график"
    Placement ||--o{ DiaryEntry : "дневник"
    Placement ||--o{ Evaluation : "оценки"
    EvaluationCriterion ||--o{ Evaluation : "по критерий"
    ApplicationUser ||--o{ Evaluation : "оценява"
    ApplicationUser ||--o{ Notification : "получава"
```

## Основни правила и ограничения

| Правило | Къде се пази |
|---------|--------------|
| Ученик кандидатства веднъж за позиция | Уникален индекс `(StudentId, InternshipPositionId)` |
| От една кандидатура става най-много едно назначаване | Уникален индекс `Placement.ApplicationId` |
| Ученик има едно активно назначаване на период | Филтриран уникален индекс `(StudentId, InternshipPeriodId)` при `Status <> Cancelled` |
| `StartDate <= EndDate` | CHECK ограничение в базата |
| Припокриващи се назначавания и графици | Бизнес логика в `Services` (SQL не може лесно да сравнява интервали) |
| Една оценка на критерий за назначаване | Уникален индекс `(PlacementId, EvaluationCriterionId)` |
| Часовете в дневника са между 0 и 12 | CHECK ограничение + валидация в услугата |
| Една заявка за връзка между двама потребители | Уникален индекс `(RequesterId, AddresseeId)` + CHECK, че не са един и същ човек |
| Едно предаване на ученик за задание | Уникален индекс `(AssignmentId, StudentId)` |
| Крайната оценка е между 2 и 6 | `FinalGrade` с точност (3,2) + валидация в услугата |
| Закъснели предавания губят 20% | Бизнес логика (`LatePolicy`, множител 0.8) |
| Файловете не са достъпни по адрес | Пазят се в `App_Data/uploads` със случайно име; изтеглянето минава през контролер с проверка на достъпа |

## Решения, които да можеш да обясниш

- **Placement има и `StudentId`, и `InternshipPeriodId`**, въпреки че ги има през кандидатурата и позицията. Дублирането е нарочно: така базата може да наложи уникалност с индекс.
- **Един `ApplicationUser` за всички роли.** Ролите са на Identity, а `OrganizationId` е празно за ученици и координатори.
- **Изтриването е ограничено (Restrict)**, за да не изчезват оценки и дневник заради изтрита организация.
