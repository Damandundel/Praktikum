# Praktikum

**Система за управление на ученически стажове и практическо обучение** – дипломен проект
(ПГДН „СофтУни Будител“, специалност „Приложно програмиране“).

Уеб системата обхваща целия процес: от кандидатстването за стажантска позиция до
отчитането и оценяването на изпълнените дейности.

## Роли

| Роля | Какво прави |
|------|-------------|
| Администратор | Управлява потребители, роли, периоди и системни настройки |
| Координатор (учител) | Периоди, организации, критерии, одобрява кандидатури, назначава наставник и график, справки |
| Работодател | Профил на организацията и публикуване на позиции |
| Наставник | Потвърждава записите в дневника, оценява по критерии |
| Ученик | Разглежда позиции, кандидатства, води дневник, вижда график и оценки |

## Технологии

C# / ASP.NET Core MVC (.NET 8) · Entity Framework Core (Code First) · MS SQL Server ·
ASP.NET Core Identity (cookie автентикация) · HTML5 / CSS3 / JavaScript ·
xUnit, Moq, coverlet, ReportGenerator · GitHub Actions

## Архитектура

```
Praktikum.sln
├── src/
│   ├── Praktikum.Web/          # MVC: Controllers, Views, ViewModels, wwwroot
│   ├── Praktikum.Services/     # Бизнес логика
│   ├── Praktikum.Data/         # DbContext, миграции, seeding, хранилища
│   ├── Praktikum.Data.Models/  # Entities и enum-и
│   └── Praktikum.Common/       # Константи, изключения, помощни класове
├── tests/
│   ├── Praktikum.Services.Tests/        # Unit тестове
│   └── Praktikum.Web.IntegrationTests/  # Интеграционни тестове
└── docs/                       # Документация и диаграми
```

Поток на заявката: `Controller → Service → Repository → DbContext`
(връзките се подават чрез Dependency Injection).

Зависимости между проектите (стрелката означава „използва“):
`Web → Services → Data → Data.Models`, а `Common` се ползва от всички слоеве.

## Локално стартиране

Изисквания: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) и Docker.

```bash
# 1. SQL Server в Docker (паролата е само за локална разработка)
docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD="<вашата-парола>" -p 1450:1433 -v praktikum-sql-data:/var/opt/mssql --name praktikum-sql -d mcr.microsoft.com/mssql/server:2022-latest

# 2. Connection string се пази в User Secrets, никога в хранилището
dotnet user-secrets init --project src/Praktikum.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1450;Database=Praktikum;User Id=sa;Password=<вашата-парола>;TrustServerCertificate=True" --project src/Praktikum.Web

# 3. Прилагане на миграциите
dotnet ef database update -p src/Praktikum.Data -s src/Praktikum.Data --connection "<connection-string>"

# 4. Стартиране
dotnet run --project src/Praktikum.Web
```

Миграциите са в `src/Praktikum.Data/Migrations`. Нова миграция се създава така:

```bash
dotnet ef migrations add <ИмеНаМиграция> -p src/Praktikum.Data -s src/Praktikum.Data -o Migrations
```

> Автоматичното прилагане на миграциите и seeding се добавят в следваща стъпка.

## Тестове

```bash
dotnet test --collect:"XPlat Code Coverage"
```

CI изпълнява тестовете автоматично и публикува отчет за покритието (виж `.github/workflows/ci.yml`).

## Сигурност на конфигурацията

В хранилището няма пароли, ключове и connection strings. За разработка се ползват
User Secrets, а за продукция – променливи на средата на хостинга.

## Демо акаунти

При първо стартиране приложението прилага миграциите и попълва базата с измислени
демо данни (без реални лични данни). Паролата е **публична и само за демо**; тя не е
свързана с никакви реални акаунти или услуги.

| Роля | Имейл | Парола |
|------|-------|--------|
| Администратор | `admin@praktikum.demo` | `Demo#2027pass` |
| Координатор | `coordinator@praktikum.demo` | `Demo#2027pass` |
| Работодател | `employer@praktikum.demo` | `Demo#2027pass` |
| Наставник | `mentor@praktikum.demo` | `Demo#2027pass` |
| Ученик | `student@praktikum.demo` | `Demo#2027pass` |

Демо данни: 3 организации, 2 периода (текущ и следващ), 4 позиции, 5 критерия за оценяване.
Ученикът има одобрена кандидатура с назначение в текущия период (график понеделник,
сряда и петък, 9 записа в дневника, от които 2 чакат потвърждение от наставника) и
една чакаща кандидатура за следващия период. Датите са спрямо деня на първото
стартиране, така че текущият период винаги е в ход.
