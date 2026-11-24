using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Praktikum.Web.Models;

namespace Praktikum.Web.Controllers;

/// <summary>
/// Персонализирани страници за грешки (400, 403, 404, 500).
/// [IgnoreAntiforgeryToken]: след отказан POST (например липсващ токен) страницата се изпълнява наново със същия
/// метод POST; без този атрибут антифалшификационната защита би я блокирала и потребителят би видял празна страница.
/// Страницата само показва текст и не променя нищо, затова е безопасно.
/// </summary>
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class ErrorController : Controller
{
    [Route("Error/{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(int statusCode)
    {
        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };

        switch (statusCode)
        {
            case 400:
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return View("BadRequest", model);
            case 403:
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return View("Forbidden", model);
            case 404:
                Response.StatusCode = StatusCodes.Status404NotFound;
                return View("NotFound", model);
            default:
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return View("ServerError", model);
        }
    }
}
