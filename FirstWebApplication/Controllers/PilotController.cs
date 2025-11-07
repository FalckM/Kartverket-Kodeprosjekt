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

        // POST: Quick Register - Save location only
        [HttpPost]
        public async Task<IActionResult> QuickRegister(string obstacleGeometry)
        {
            if (string.IsNullOrEmpty(obstacleGeometry))
            {
                TempData["ErrorMessage"] = "Please mark the obstacle location on the map";
                return View();
            }

            var userEmail = User.Identity?.Name;

            var obstacle = new ObstacleData
            {
                ObstacleGeometry = obstacleGeometry,
                RegisteredBy = userEmail,
                RegisteredDate = DateTime.Now,
                // Leave these null for now - will be filled later
                ObstacleName = null,
                ObstacleHeight = 0,
                ObstacleDescription = null
            };

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Location saved! Please complete the registration.";
            return RedirectToAction("CompleteQuickRegister", new { id = obstacle.Id });
        }

        // GET: Full Register - Complete form with location
        [HttpGet]
        public IActionResult FullRegister()
        {
            return View();
        }

        // POST: Full Register - Save everything at once
        [HttpPost]
        public async Task<IActionResult> FullRegister(ObstacleData obstacle)
        {
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

        // GET: Complete Quick Register - Fill in missing details
        [HttpGet]
        public async Task<IActionResult> CompleteQuickRegister(int id)
        {
            var userEmail = User.Identity?.Name;
            var obstacle = await _context.Obstacles
                .Where(o => o.Id == id && o.RegisteredBy == userEmail)
                .FirstOrDefaultAsync();

            if (obstacle == null)
            {
                return NotFound();
            }

            return View(obstacle);
        }

        // POST: Save completed quick register
        [HttpPost]
        public async Task<IActionResult> SaveCompleteQuickRegister(int id, ObstacleData model)
        {
            var userEmail = User.Identity?.Name;

            // Find the existing obstacle
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            // Verify ownership
            if (obstacle.RegisteredBy != userEmail)
            {
                return Forbid();
            }

            // Update with completed information
            obstacle.ObstacleName = model.ObstacleName;
            obstacle.ObstacleHeight = model.ObstacleHeight;
            obstacle.ObstacleDescription = model.ObstacleDescription;
            obstacle.ObstacleType = model.ObstacleType;

            // Manually validate the updated obstacle
            // Clear existing model state
            ModelState.Clear();

            // Validate the obstacle object
            TryValidateModel(obstacle);

            if (!ModelState.IsValid)
            {
                // Return to form with validation errors
                return View("CompleteQuickRegister", obstacle);
            }

            // Save to database
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration completed successfully!";
            return RedirectToAction("MyRegistrations");
        }

        // GET: My Registrations - View all obstacles by current user
        [HttpGet]
        public async Task<IActionResult> MyRegistrations()
        {
            var userEmail = User.Identity?.Name;

            var obstacles = await _context.Obstacles
                .Where(o => o.RegisteredBy == userEmail)
                .OrderByDescending(o => o.RegisteredDate)
                .ToListAsync();

            return View(obstacles);
        }

        // GET: Overview - View single obstacle details
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

            // Check if this is a new registration (from TempData)
            ViewBag.IsNewRegistration = TempData["IsNewRegistration"] as bool? ?? false;

            return View(obstacle);
        }

        // POST: Delete registration (only if pending)
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

            // Only allow deletion if not approved or rejected
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

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}