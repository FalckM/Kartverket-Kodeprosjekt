using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DataForm(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
            {
                return View(obstacleData);
            }
            return View("Overview", obstacleData);
        }
    }
}
