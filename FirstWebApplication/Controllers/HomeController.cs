using FirstWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FirstWebApplication.Controllers
{
    // [Authorize] means users MUST be logged in to access this controller
    // If not logged in, they will be redirected to the AuthPage
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET: /Home/Index
        // Home page - only accessible to logged-in users
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Privacy
        // Privacy page - only accessible to logged-in users
        public IActionResult Privacy()
        {
            return View();
        }

        // Error page - accessible without login
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}