using FirstWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FirstWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET: /Home/Index
        // This is the landing page - shows the map with login overlay
        // Anyone can access this page (no [Authorize] attribute)
        [AllowAnonymous]
        public IActionResult Index()
        {
            // If user is already logged in, redirect them directly to the workspace
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Workspace", "Obstacle");
            }

            // User is not logged in, show the landing page with login modal
            return View();
        }

        // GET: /Home/Privacy
        // Privacy page - only accessible to logged-in users
        [Authorize]
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