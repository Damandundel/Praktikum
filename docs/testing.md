# Тестване и CI – Praktikum

## Нива на тестване

| Ниво | Проект | Брой | Какво проверява |
|------|--------|-----:|-----------------|
| Unit | `tests/Praktikum.Services.Tests` | 227 | Бизнес правилата в `Praktikum.Services` изолирано, с база в паметта (EF Core InMemory) и фиксирано „днес“ (`FakeTimeProvider`) |
| Интеграционни | `tests/Praktikum.Web.IntegrationTests` | 164 | Цялото приложение през HTTP (`WebApplicationFactory`): middleware, контролери, Identity, антифалшификационни токени, изгледи и SQLite база |
| Ръчни | таблица по-долу | – | Устройства и браузъри (**още не са направени**) |

Инструменти: xUnit, `Microsoft.AspNetCore.Mvc.Testing`, EF Core InMemory и SQLite, coverlet (покритие), ReportGenerator (отчет).

## Какво е покрито спрямо заданието

| Изискване | Къде |
|-----------|------|
| Одобряване на кандидатура | `ApplicationServiceTests` (Approve_*) |
| Застъпване на стажове и графици | `ApplicationServiceTests` (Approve_Overlapping*, Approve_PeriodsTouching*), `PlacementServiceTests` (Assign_WhenStudentHasOverlapping*, AddSlot_Overlapping*) и интеграционният `OverlappingPeriods_SecondApprovalIsRejected` |
| Потвърждаване на записи от наставник | `DiaryServiceTests` (Confirm_*, Reject_*) |
| Изчисляване на часове | `DiaryServiceTests` (DiarySummary_*, GetProgress_*), `ReportServiceTests` |
| Изчисляване на оценка (Strategy) | `GradeStrategyTests`, `EvaluationServiceTests` |
| Основен процес: кандидатстване → одобрение → назначаване → дневник → потвърждение → оценка | `FullWorkflowTests.StudentJourney_FromApplicationToGradeInReport` (11 стъпки, включително справка и CSV) |
| Роли и права | `SecurityTests`: достъп без вход (17 адреса) и 81 комбинации роля × страница (29 допустими и 52 забранени) |
| CSRF | `SecurityTests`: POST без токен и с фалшив токен връщат 400 |
| Защитена бисквитка | `SecurityTests.LoginCookie_IsHttpOnlySecureAndSameSiteLax` |
| Заключване на профил, генерични съобщения за вход | `SecurityTests` (5 грешни опита, един и същ текст за грешен имейл и парола) |
| Достъп до чужди данни (IDOR) | `SecurityTests`: чужд ученик, работодател и наставник получават 404 или не променят нищо |
| **SQL Injection** | `InjectionTests`: 7 payload-а в търсенето, филтрите на справката, входа, регистрацията и формите. Проверява се: няма грешка 500, условието не се разширява (`' OR '1'='1` не връща всички позиции) и таблиците са непокътнати |
| **XSS** | `XssTests`: 3 вида payload (`<script>`, `"><img onerror>`, `<svg onload>`) през името, периода, организацията, критерия, позицията, мотивационното писмо, дневника, коментарите и параметъра `returnUrl`. На всяка страница, където се показват, суровият HTML го няма, а специалните знаци са кодирани |
| CSV injection | `ReportCsvTests`: клетки, започващи с `=`, `+`, `-`, `@`, се обезопасяват |

## Как се пускат

```bash
dotnet test                                                # всички тестове
dotnet test tests/Praktikum.Services.Tests                 # само unit
dotnet test tests/Praktikum.Web.IntegrationTests           # само интеграционни
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool restore                                        # инсталира ReportGenerator и dotnet-ef от .config/dotnet-tools.json
dotnet reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html"
```
Отчетът е в `coveragereport/index.html`. Тестовете не изискват Docker или SQL Server.

## Покритие

| Измерване | Резултат | Изискване |
|-----------|---------:|----------:|
| `Praktikum.Services` само от unit тестове (праг в CI) | 95.9% | минимум 65% |
| `Praktikum.Services` от unit + интеграционни | 97.9% | – |

Покритието на целия проект е по-ниско (около 50%), защото включва генерирания код на миграциите и сидирането. Изискването на заданието се отнася за бизнес логиката, затова CI проверява `Praktikum.Services`.

## Как работят интеграционните тестове

`PraktikumFactory` стартира приложението в паметта на теста (среда `Testing`, така че User Secrets не се четат) и заменя SQL Server със **SQLite в паметта**. Всеки тестов клас получава своя изолирана база, сидирана от същите сидъри като в продукцията, затова тестовете ползват демо акаунтите. `AppClient` работи като браузър: пази бисквитките, взема антифалшификационния токен от страницата и го връща с формата.

**Ограничения (честно):**
- SQLite не е SQL Server. Не поддържа `SUM` върху `decimal`, затова в тестовата фабрика `decimal` се съхранява като `double` (само там). Миграциите са писани за SQL Server и в тестовете се пропускат (`Database:ApplyMigrations=false`, базата се създава от модела).
- Затова втори job в CI проверява на истински SQL Server: миграциите се прилагат върху празна база, моделът съвпада с миграциите и приложението стартира и сидира 5 демо потребители.
- Търсенето в InMemory/SQLite различава главни и малки букви за кирилица, а SQL Server – не. Unit тестовете за търсене ползват точен регистър.

**Проверка, че тестовете могат да паднат (mutation check):** временно изключих проверката за застъпване на периоди → падна `OverlappingPeriods_SecondApprovalIsRejected`; временно показах заглавие на позиция с `Html.Raw` → паднаха и трите XSS теста. И двете промени са върнати.

## CI (GitHub Actions) – `.github/workflows/ci.yml`

| Job | Стъпки |
|-----|--------|
| **Build, тестове и покритие** | restore (с кеш на NuGet) → build Release → unit тестове с покритие → интеграционни тестове с покритие → отчет (ReportGenerator) → **праг 65% за `Praktikum.Services` (само unit)** → обобщение в страницата на изпълнението → артефакти `coverage-report` и `test-results` (.trx) |
| **Миграции и старт върху SQL Server** | SQL Server в контейнер с генерирана еднократна парола (маскирана, не е в хранилището) → `has-pending-model-changes` (пада, ако промениш entity без миграция) → `dotnet ef database update` върху празна база → старт на приложението в Production и проверка на 5-те демо потребители |
| Деплой | **още не е добавен** – след избор на хостинг (Етап 3) |

Версиите на ReportGenerator и dotnet-ef са фиксирани в `.config/dotnet-tools.json`, така че CI и локалната среда ползват едни и същи.

**Какво е проверено за CI:** файлът е валиден YAML; всички `run` стъпки са извлечени и изпълнени локално в Git Bash – build, двата тестови проекта, отчетът, прагът (минава при 65%, пада при 99%), стартът на SQL Server в Docker, миграциите, моделът и стартът на приложението. **Не е пускан в GitHub** – реалното изпълнение там още не е видяно.
Локалните проби изискваха две замени, които не са нужни на Linux: друг порт на SQL Server (1433 беше зает) и `NUL` вместо `/dev/null`.

## Ръчно тестване (таблица за документацията – попълва се в Етап 4)

| Устройство | Браузър | Вход/регистрация | Позиции и кандидатстване | Дневник | Оценки | Справки | Бележки |
|------------|---------|:----:|:----:|:----:|:----:|:----:|---------|
| Настолен компютър | Chrome | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Настолен компютър | Firefox | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Настолен компютър | Edge | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Телефон (Android) | Chrome | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Телефон (iPhone) | Safari | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Таблет | Chrome/Safari | ☐ | ☐ | ☐ | ☐ | ☐ | |

## Открити грешки при писането на тестовете
- **Страница 400 без съдържание.** При отказан POST (например липсващ токен) страницата за грешки се изпълнява наново като POST и антифалшификационната защита я блокираше – потребителят виждаше празна страница. Поправено с `[IgnoreAntiforgeryToken]` в `ErrorController` и проверено на реалния сървър.
- **`dotnet run` в CI.** Без `--no-launch-profile` ползва `launchSettings.json` (Development, други портове) и игнорира `ASPNETCORE_URLS`. Поправено в workflow-а.
