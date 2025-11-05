using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    // Only users with "Pilot" role can access this controller
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

        // GET: /Pilot/Dashboard
        // Landing page for pilots after login
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: /Pilot/RegisterType
        // Choose between Quick Register or Full Register
        [HttpGet]
        public IActionResult RegisterType()
        {
            return View();
        }

        // GET: /Pilot/QuickRegister
        // Quick registration - capture location only
        [HttpGet]
        public IActionResult QuickRegister()
        {
            return View();
        }

        // POST: /Pilot/SaveQuickRegister
        // Save quick registration
        [HttpPost]
        public async Task<IActionResult> SaveQuickRegister([FromBody] QuickRegisterRequest request)
        {
            try
            {
                var obstacle = new ObstacleData
                {
                    ObstacleGeometry = request.ObstacleGeometry,
                    RegisteredBy = User.Identity?.Name ?? "Unknown",
                    RegisteredDate = DateTime.Now,
                    ObstacleName = "",
                    ObstacleHeight = 0,
                    ObstacleDescription = "",
                    IsApproved = false // New obstacles need approval
                };

                _context.Obstacles.Add(obstacle);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = obstacle.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save quick registration");
                return Json(new { success = false, message = "Failed to save location" });
            }
        }

        // GET: /Pilot/MyRegistrations
        // Show all obstacles registered by this pilot
        [HttpGet]
        public async Task<IActionResult> MyRegistrations()
        {
            var userEmail = User.Identity?.Name;
            var myObstacles = await _context.Obstacles
                .Where(o => o.RegisteredBy == userEmail)
                .OrderByDescending(o => o.RegisteredDate)
                .ToListAsync();

            return View(myObstacles);
        }

        // GET: /Pilot/CompleteQuickRegister/5
        // Complete a quick-registered obstacle
        [HttpGet]
        public async Task<IActionResult> CompleteQuickRegister(int? id)
        {
            if (id == null)
                return NotFound();

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
                return NotFound();

            // Security: only the pilot who registered it can complete it
            if (obstacle.RegisteredBy != User.Identity?.Name)
                return Forbid();

            return View(obstacle);
        }

        // POST: /Pilot/SaveCompleteQuickRegister
        [HttpPost]
        public async Task<IActionResult> SaveCompleteQuickRegister(ObstacleData obstacleData)
        {
            ModelState.Remove("ObstacleGeometry");

            if (!ModelState.IsValid)
                return View("CompleteQuickRegister", obstacleData);

            var existingObstacle = await _context.Obstacles.FindAsync(obstacleData.Id);

            if (existingObstacle == null)
                return NotFound();

            if (existingObstacle.RegisteredBy != User.Identity?.Name)
                return Forbid();

            existingObstacle.ObstacleName = obstacleData.ObstacleName;
            existingObstacle.ObstacleHeight = obstacleData.ObstacleHeight;
            existingObstacle.ObstacleDescription = obstacleData.ObstacleDescription;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration completed! Waiting for approval from Registerfører.";
            return RedirectToAction("Overview", new { id = existingObstacle.Id });
        }

        // GET: /Pilot/DeleteRegistration/5
        [HttpGet]
        public async Task<IActionResult> DeleteRegistration(int? id)
        {
            if (id == null)
                return NotFound();

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
                return NotFound();

            if (obstacle.RegisteredBy != User.Identity?.Name)
                return Forbid();

            _context.Obstacles.Remove(obstacle);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration deleted successfully";
            return RedirectToAction("MyRegistrations");
        }

        // GET: /Pilot/FullRegister
        [HttpGet]
        public IActionResult FullRegister()
        {
            return View(new ObstacleData());
        }

        // POST: /Pilot/SaveFullRegister
        [HttpPost]
        public async Task<IActionResult> SaveFullRegister(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
                return View("FullRegister", obstacleData);

            obstacleData.RegisteredBy = User.Identity?.Name ?? "Unknown";
            obstacleData.RegisteredDate = DateTime.Now;
            obstacleData.IsApproved = false; // Needs approval

            _context.Obstacles.Add(obstacleData);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration submitted! Waiting for approval from Registerfører.";
            return RedirectToAction("Overview", new { id = obstacleData.Id });
        }

        // GET: /Pilot/Overview/5
        [HttpGet]
        public async Task<IActionResult> Overview(int? id)
        {
            if (id == null)
                return NotFound();

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
                return NotFound();

            // Pilots can only view their own obstacles
            if (obstacle.RegisteredBy != User.Identity?.Name)
                return Forbid();

            return View(obstacle);
        }
        public class QuickRegisterRequest
        {
            public string ObstacleGeometry { get; set; } = string.Empty;
        }
    }
}