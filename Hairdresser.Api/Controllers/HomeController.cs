using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Api.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard");
    }
}