using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Pilot")]
    public class PilotController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PilotController> _logger;

        public PilotController(ApplicationDbContext context, ILogger<PilotController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Choose registration type
        [HttpGet]
        public IActionResult RegisterType()
        {
            return View();
        }

        // GET: Quick Register - Save location first
        [HttpGet]
        public IActionResult QuickRegister()
        {
            return View();
        }

        // ============================================================
        // ENDRET: Lagrer nå i QuickRegistrations i stedet for Obstacles
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> QuickRegister(string obstacleGeometry)
        {
            if (string.IsNullOrEmpty(obstacleGeometry))
            {
                TempData["ErrorMessage"] = "Please mark the obstacle location on the map";
                return View();
            }

            var userEmail = User.Identity?.Name;

            // Lagre i QuickRegistrations (IKKE Obstacles!)
            var quickReg = new QuickRegistration
            {
                ObstacleGeometry = obstacleGeometry,
                RegisteredBy = userEmail,
                RegisteredDate = DateTime.Now,
                IsCompleted = false
            };

            _context.QuickRegistrations.Add(quickReg);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Location saved! Please complete the registration.";
            return RedirectToAction("RegisterType");
        }

        // GET: Full Register - Complete form with location
        [HttpGet]
        public IActionResult FullRegister()
        {
            return View();
        }

        // POST: Full Register - Save everything at once
        // INGEN ENDRINGER HER - lagrer fortsatt direkte i Obstacles
        [HttpPost]
        public async Task<IActionResult> FullRegister(ObstacleData obstacle)
        {
            // Manuell validering for Full Register
            if (string.IsNullOrWhiteSpace(obstacle.ObstacleName))
            {
                ModelState.AddModelError("ObstacleName", "Obstacle name is required");
            }

            if (obstacle.ObstacleHeight <= 0)
            {
                ModelState.AddModelError("ObstacleHeight", "Height must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(obstacle.ObstacleDescription))
            {
                ModelState.AddModelError("ObstacleDescription", "Description is required");
            }

            if (string.IsNullOrWhiteSpace(obstacle.ObstacleGeometry))
            {
                ModelState.AddModelError("ObstacleGeometry", "Please mark the location on the map");
            }

            if (!ModelState.IsValid)
            {
                return View(obstacle);
            }

            var userEmail = User.Identity?.Name;
            obstacle.RegisteredBy = userEmail;
            obstacle.RegisteredDate = DateTime.Now;

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            TempData["IsNewRegistration"] = true;
            return RedirectToAction("Overview", new { id = obstacle.Id });
        }

        // ============================================================
        // ENDRET: Henter nå fra QuickRegistrations
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> CompleteQuickRegister(int id)
        {
            var userEmail = User.Identity?.Name;

            // Hent QuickRegistration (ikke Obstacle!)
            var quickReg = await _context.QuickRegistrations
                .Where(q => q.Id == id && q.RegisteredBy == userEmail && !q.IsCompleted)
                .FirstOrDefaultAsync();

            if (quickReg == null)
            {
                TempData["ErrorMessage"] = "Quick registration not found or already completed.";
                return RedirectToAction("MyRegistrations");
            }

            // Lag en ny ObstacleData med geometry fra QuickRegistration
            var obstacle = new ObstacleData
            {
                ObstacleGeometry = quickReg.ObstacleGeometry,
                RegisteredBy = userEmail,
                RegisteredDate = quickReg.RegisteredDate // Behold original dato
            };

            // Send QuickReg ID til view
            ViewBag.QuickRegId = id;
            return View(obstacle);
        }

        // ============================================================
        // ENDRET: Flytter data fra QuickRegistrations til Obstacles
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> SaveCompleteQuickRegister(int quickRegId, ObstacleData model)
        {
            var userEmail = User.Identity?.Name;

            // Finn QuickRegistration
            var quickReg = await _context.QuickRegistrations
                .Where(q => q.Id == quickRegId && q.RegisteredBy == userEmail && !q.IsCompleted)
                .FirstOrDefaultAsync();

            if (quickReg == null)
            {
                TempData["ErrorMessage"] = "Quick registration not found or already completed.";
                return RedirectToAction("MyRegistrations");
            }

            // Manuell validering
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(model.ObstacleName))
            {
                ModelState.AddModelError("ObstacleName", "Obstacle name is required");
            }

            if (model.ObstacleHeight <= 0)
            {
                ModelState.AddModelError("ObstacleHeight", "Height must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(model.ObstacleDescription))
            {
                ModelState.AddModelError("ObstacleDescription", "Description is required");
            }

            if (!ModelState.IsValid)
            {
                // Behold geometry og metadata
                model.ObstacleGeometry = quickReg.ObstacleGeometry;
                model.RegisteredBy = userEmail;
                model.RegisteredDate = quickReg.RegisteredDate;
                ViewBag.QuickRegId = quickRegId;
                return View("CompleteQuickRegister", model);
            }

            // Opprett fullstendig ObstacleData
            var obstacle = new ObstacleData
            {
                ObstacleName = model.ObstacleName,
                ObstacleHeight = model.ObstacleHeight,
                ObstacleDescription = model.ObstacleDescription,
                ObstacleType = model.ObstacleType,
                ObstacleGeometry = quickReg.ObstacleGeometry, // Fra QuickReg
                RegisteredBy = userEmail,
                RegisteredDate = quickReg.RegisteredDate // Behold original dato
            };

            // Lagre i Obstacles
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // Marker QuickRegistration som completed
            quickReg.IsCompleted = true;
            quickReg.CompletedObstacleId = obstacle.Id;
            _context.QuickRegistrations.Update(quickReg);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration completed successfully!";
            return RedirectToAction("MyRegistrations");
        }

        // ============================================================
        // ENDRET: Viser både Obstacles og ufullstendige QuickRegistrations
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> MyRegistrations()
        {
            var userEmail = User.Identity?.Name;

            // Hent fullstendige obstacles
            var obstacles = await _context.Obstacles
                .Where(o => o.RegisteredBy == userEmail)
                .OrderByDescending(o => o.RegisteredDate)
                .ToListAsync();

            // Hent ufullstendige quick registrations
            var incompleteQuickRegs = await _context.QuickRegistrations
                .Where(q => q.RegisteredBy == userEmail && !q.IsCompleted)
                .OrderByDescending(q => q.RegisteredDate)
                .ToListAsync();

            // Send begge til view
            ViewBag.IncompleteQuickRegs = incompleteQuickRegs;
            return View(obstacles);
        }

        // GET: Overview - View single obstacle details
        // INGEN ENDRINGER HER
        [HttpGet]
        public async Task<IActionResult> Overview(int id)
        {
            var userEmail = User.Identity?.Name;

            var obstacle = await _context.Obstacles
                .Where(o => o.Id == id && o.RegisteredBy == userEmail)
                .FirstOrDefaultAsync();

            if (obstacle == null)
            {
                return NotFound();
            }

            ViewBag.IsNewRegistration = TempData["IsNewRegistration"] as bool? ?? false;
            return View(obstacle);
        }

        // POST: Delete registration (only if pending)
        // INGEN ENDRINGER HER
        [HttpPost]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            var userEmail = User.Identity?.Name;

            var obstacle = await _context.Obstacles
                .Where(o => o.Id == id && o.RegisteredBy == userEmail)
                .FirstOrDefaultAsync();

            if (obstacle == null)
            {
                return NotFound();
            }

            if (obstacle.IsApproved || obstacle.IsRejected)
            {
                TempData["ErrorMessage"] = "Cannot delete obstacles that have been approved or rejected";
                return RedirectToAction("MyRegistrations");
            }

            _context.Obstacles.Remove(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration deleted successfully";
            return RedirectToAction("MyRegistrations");
        }

        // ============================================================
        // NY METODE: Slett ufullstendig QuickRegistration
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> DeleteQuickRegistration(int id)
        {
            var userEmail = User.Identity?.Name;

            var quickReg = await _context.QuickRegistrations
                .Where(q => q.Id == id && q.RegisteredBy == userEmail && !q.IsCompleted)
                .FirstOrDefaultAsync();

            if (quickReg == null)
            {
                TempData["ErrorMessage"] = "Quick registration not found or already completed";
                return RedirectToAction("MyRegistrations");
            }

            _context.QuickRegistrations.Remove(quickReg);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Quick registration deleted successfully";
            return RedirectToAction("MyRegistrations");
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        // ============================================================
        // DUPLIKATSJEKK - INGEN ENDRINGER
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> CheckDuplicates(double latitude, double longitude, double radiusMeters = 10)
        {
            try
            {
                var allObstacles = await _context.Obstacles.ToListAsync();
                var nearbyObstacles = new List<object>();

                foreach (var obstacle in allObstacles)
                {
                    var geometry = obstacle.ObstacleGeometry.Replace("POINT(", "").Replace(")", "");
                    var coords = geometry.Split(' ');

                    if (coords.Length == 2 &&
                        double.TryParse(coords[0], out double obsLon) &&
                        double.TryParse(coords[1], out double obsLat))
                    {
                        double distance = CalculateDistance(latitude, longitude, obsLat, obsLon);

                        if (distance <= radiusMeters)
                        {
                            nearbyObstacles.Add(new
                            {
                                id = obstacle.Id,
                                type = obstacle.ObstacleType ?? "Ukjent type",
                                name = obstacle.ObstacleName ?? "Ingen navn",
                                height = obstacle.ObstacleHeight,
                                description = obstacle.ObstacleDescription ?? "Ingen beskrivelse",
                                distance = Math.Round(distance, 1),
                                latitude = obsLat,
                                longitude = obsLon
                            });
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    count = nearbyObstacles.Count,
                    obstacles = nearbyObstacles.OrderBy(o => ((dynamic)o).distance).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicate obstacles");
                return Json(new { success = false, error = ex.Message });
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            double lat1Rad = lat1 * Math.PI / 180;
            double lat2Rad = lat2 * Math.PI / 180;
            double deltaLat = (lat2 - lat1) * Math.PI / 180;
            double deltaLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                      Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                      Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c;

            return distance;
        }
    }
}