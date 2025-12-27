using Microsoft.AspNetCore.Mvc;

namespace BookingAPI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}

