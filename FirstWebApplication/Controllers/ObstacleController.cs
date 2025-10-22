using FirstWebApplication.Models;
using FirstWebApplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        // Dependency Injection: Vi "injiserer" database-konteksten inn i kontrolleren
        // Dette gjør at vi kan bruke databasen i alle metodene våre
        private readonly ApplicationDbContext _context;

        // Constructor som tar imot ApplicationDbContext
        // ASP.NET Core gir oss automatisk riktig DbContext når kontrolleren lages
        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Viser skjemaet for registrering (GET)
        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        // Håndterer innsending av skjemaet (POST)
        [HttpPost]
        public async Task<IActionResult> DataForm(ObstacleData obstacleData)
        {
            // Sjekker om datamodellen er gyldig (validering)
            if (!ModelState.IsValid)
            {
                return View(obstacleData);
            }

            // Legger til det nye hinderet i databasen
            // _context.Obstacles.Add() legger objektet til i minnet
            _context.Obstacles.Add(obstacleData);

            // SaveChangesAsync() skriver endringene til databasen
            // await betyr at vi venter på at operasjonen fullføres
            await _context.SaveChangesAsync();

            // TempData overlever en redirect (i motsetning til ViewBag)
            TempData["IsNewRegistration"] = true;

            // Sender brukeren videre til Overview-siden med det nye ID-et
            return RedirectToAction("Overview", new { id = obstacleData.Id });
        }

        // NY METODE: Viser Overview (fungerer både for nye og eksisterende hindre)
        [HttpGet]
        public async Task<IActionResult> Overview(int? id)
        {
            // Sjekker om ID er null eller ugyldig
            if (id == null)
            {
                return NotFound();
            }

            // FindAsync() søker etter et hinder med det spesifikke ID-et
            var obstacle = await _context.Obstacles.FindAsync(id);

            // Hvis hinderet ikke finnes, returner 404 Not Found
            if (obstacle == null)
            {
                return NotFound();
            }

            // Setter ViewBag basert på om det er en ny registrering eller ikke
            // TempData blir slettet etter at den er lest én gang
            ViewBag.IsNewRegistration = TempData["IsNewRegistration"] as bool? ?? false;

            // Sender hinderet til Overview-viewet
            return View(obstacle);
        }

        // NY METODE: Viser liste over alle hindre
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ToListAsync() henter alle hindre fra databasen som en liste
            var obstacles = await _context.Obstacles.ToListAsync();
            return View(obstacles);
        }
    }
}