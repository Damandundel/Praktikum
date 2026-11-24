using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using Praktikum.Data;
using Praktikum.Data.Models;
using Praktikum.Data.Repositories;
using Praktikum.Data.Seeding;
using Praktikum.Services;
using Praktikum.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Локален файл за разработка (в .gitignore, никога не се commit-ва).
builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: false);

// Connection string идва от User Secrets (разработка) или от променливите на средата (продукция).
// Никога не се записва в appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Липсва connection string 'DefaultConnection'. Задайте го с 'dotnet user-secrets set' (виж README).");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ASP.NET Core Identity: потребители и роли, съхранявани в нашата база.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<BulgarianIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

// Сесийна бисквитка: HttpOnly, Secure и SameSite=Lax (защита от кражба на сесията и CSRF).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Error/403";
});

// Разрешава кирилица в HTML, като <, >, &, " и ' продължават да се кодират (защита от XSS).
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic));

// Dependency Injection: Controller -> Service -> UnitOfWork/Repository -> DbContext.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPositionService, PositionService>();

builder.Services.AddAuthorization(options =>
{
    // Сигурност по подразбиране: всяка страница изисква вход, освен ако не е маркирана с [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddControllersWithViews(options =>
{
    // Всички POST/PUT/DELETE заявки изискват антифалшификационен токен (защита от CSRF).
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Прилага миграциите и попълва ролите и демо данните (сидърите са идемпотентни).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Миграциите са писани за SQL Server. Интеграционните тестове ползват SQLite в паметта
    // и изключват миграциите ("Database:ApplyMigrations": false), като базата се създава от модела.
    if (builder.Configuration.GetValue("Database:ApplyMigrations", true))
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    await new ApplicationDbContextSeeder().SeedAsync(dbContext, scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

// Заявки със статус 403/404 и др. се пренасочват към ErrorController (адресът в браузъра не се променя).
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Фиксирана (инвариантна) култура за числата и датите във формите.
var invariantCulture = new[] { CultureInfo.InvariantCulture };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
    SupportedCultures = invariantCulture,
    SupportedUICultures = invariantCulture,
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>Прави входната точка видима за тестовете (WebApplicationFactory&lt;Program&gt;).</summary>
public partial class Program
{
}
