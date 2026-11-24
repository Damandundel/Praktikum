using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Praktikum.Common;
using Praktikum.Data.Models;
using Praktikum.Web.ViewModels.Account;

namespace Praktikum.Web.Controllers;

/// <summary>Вход, изход и регистрация на ученици.</summary>
public class AccountController : Controller
{
    // Едно общо съобщение за грешен имейл и грешна парола, за да не издаваме кои имейли съществуват.
    private const string InvalidLoginMessage = "Грешен имейл или парола.";

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());

        // Деактивиран потребител не може да влезе, но получава същото общо съобщение.
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, InvalidLoginMessage);
            return View(model);
        }

        // lockoutOnFailure: след 5 грешни опита профилът се заключва за кратко (защита от налучкване на пароли).
        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Потребител {UserId} влезе в системата.", user.Id);
            return RedirectToLocal(model.ReturnUrl);
        }

        ModelState.AddModelError(
            string.Empty,
            result.IsLockedOut
                ? "Профилът е временно заключен след няколко неуспешни опита. Опитайте отново след 5 минути."
                : InvalidLoginMessage);

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    /// <summary>
    /// Саморегистрация е позволена само за ученици. Останалите роли
    /// се създават от администратор, за да не може всеки да стане наставник или координатор.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            ClassName = model.ClassName.Trim().ToUpperInvariant().Replace(" ", string.Empty),
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, GlobalConstants.Roles.Student);
        await _signInManager.SignInAsync(user, isPersistent: false);

        _logger.LogInformation("Регистриран е нов ученик {UserId}.", user.Id);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Пренасочва само към адреси в нашия сайт (защита от open redirect).</summary>
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }
}
