using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        // Viser skjemaet for registrering (GET forespørsel)
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        // Håndterer innsending av skjemaet (POST forespørsel)
        [HttpPost]
        public IActionResult DataForm(ObstacleData obstacleData)
        {
            // Sjekker om datamodellen er gyldig (f.eks. om GeoJSON ble sendt inn)
            if (!ModelState.IsValid)
            {
                return View(obstacleData);
            }
            // Sender dataene til oversiktssiden for visning
            return View("Overview", obstacleData);
        }
    }
}



