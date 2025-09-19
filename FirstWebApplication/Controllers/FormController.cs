using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class FormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
