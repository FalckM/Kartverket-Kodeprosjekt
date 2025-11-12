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
        // This is the landing page - shows the map with login/register forms
        // Anyone can access this page (no [Authorize] attribute)
        [AllowAnonymous]
        public IActionResult Index()
        {
            // If user is already logged in, redirect them based on their role
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Check role and send to correct dashboard (same logic as Login)
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("AdminDashboard", "Admin");
                }
                else if (User.IsInRole("Registerfører"))
                {
                    return RedirectToAction("RegisterforerDashboard", "Registerforer");
                }
                else // Pilot or no role
                {
                    return RedirectToAction("RegisterType", "Pilot");
                }
            }

            // Check if we should show the register form (from failed registration)
            if (TempData["ShowRegister"] != null)
            {
                ViewBag.ShowRegister = true;

                // Get registration errors from TempData and add them to ModelState
                if (TempData["RegisterErrors"] != null)
                {
                    var errors = TempData["RegisterErrors"].ToString().Split('|');
                    foreach (var error in errors)
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            ModelState.AddModelError(string.Empty, error);
                        }
                    }
                }
            }
            // Check if we have login errors
            else if (TempData["LoginErrors"] != null)
            {
                var errors = TempData["LoginErrors"].ToString().Split('|');
                foreach (var error in errors)
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }

            // User is not logged in, show the landing page with login/register modal
            return View();
        }

        // GET: /Home/Privacy
        // Privacy page - accessible to everyone (AllowAnonymous)
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // Error page - accessible without loginn
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}