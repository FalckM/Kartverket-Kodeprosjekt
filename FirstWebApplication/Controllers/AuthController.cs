using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class AuthController : Controller
    {
        // GET /Auth/Register  -> shows the register card over the map
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST /Auth/Register  -> fake create, then go back to the login gate (Home/Index)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(object _)
        {
            // TODO: later, create user + sign-in. For now we just go back to the gate.
            return RedirectToAction("Index", "Home");
        }
    }
}