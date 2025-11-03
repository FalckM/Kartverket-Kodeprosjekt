using FirstWebApplication.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication.Entities;

namespace FirstWebApplication.Controllers
{
    // [Authorize] means users MUST be logged in to access obstacle registration
    // This ensures only authenticated users can report obstacles
    [Authorize]
    public class ObstacleController : Controller
    {
        // Dependency Injection: We "inject" the database context into the controller
        // This allows us to use the database in all our methods
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ObstacleController> _logger;

        // Constructor that receives ApplicationDbContext
        // ASP.NET Core automatically gives us the correct DbContext when the controller is created
        public ObstacleController(ApplicationDbContext context, ILogger<ObstacleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Obstacle/RegisterType
        // Shows the user two options: Quick Register or Full Register
        [HttpGet]
        public IActionResult RegisterType()
        {
            return View();
        }

        // GET: /Obstacle/QuickRegister
        // Shows the quick registration page that captures browser location
        [HttpGet]
        public IActionResult QuickRegister()
        {
            return View();
        }

        // POST: /Obstacle/SaveQuickRegister
        // Saves a quick registration with only location data (called by JavaScript)
        [HttpPost]
        public async Task<IActionResult> SaveQuickRegister([FromBody] QuickRegisterRequest request)
        {
            try
            {
                // Create a new obstacle with minimal data
                var obstacle = new ObstacleData
                {
                    ObstacleGeometry = request.ObstacleGeometry,
                    RegisteredBy = User.Identity?.Name ?? "Unknown",
                    RegisteredDate = DateTime.Now,
                    // Set placeholder values for required fields
                    ObstacleName = "", // Empty - will be filled later
                    ObstacleHeight = 0, // Placeholder
                    ObstacleDescription = "" // Empty - will be filled later
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

        // GET: /Obstacle/QuickRegisteredItems
        // Shows all quick-registered items (incomplete registrations)
        [HttpGet]
        public async Task<IActionResult> QuickRegisteredItems()
        {
            // Get the current user's email
            var userEmail = User.Identity?.Name;

            // Get all obstacles registered by this user
            // We can show both complete and incomplete, but mark incomplete ones
            var userObstacles = await _context.Obstacles
                .Where(o => o.RegisteredBy == userEmail)
                .OrderByDescending(o => o.RegisteredDate)
                .ToListAsync();

            return View(userObstacles);
        }

        // GET: /Obstacle/CompleteQuickRegister/5
        // Shows form to complete a quick-registered obstacle
        [HttpGet]
        public async Task<IActionResult> CompleteQuickRegister(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            // Security check: make sure this obstacle belongs to the current user
            if (obstacle.RegisteredBy != User.Identity?.Name)
            {
                return Forbid();
            }

            return View(obstacle);
        }

        // POST: /Obstacle/SaveCompleteQuickRegister
        // Saves the completed details for a quick-registered obstacle
        [HttpPost]
        public async Task<IActionResult> SaveCompleteQuickRegister(ObstacleData obstacleData)
        {
            // We only validate the fields we're updating
            ModelState.Remove("ObstacleGeometry"); // Already set

            if (!ModelState.IsValid)
            {
                return View("CompleteQuickRegister", obstacleData);
            }

            // Find the existing obstacle in the database
            var existingObstacle = await _context.Obstacles.FindAsync(obstacleData.Id);

            if (existingObstacle == null)
            {
                return NotFound();
            }

            // Security check
            if (existingObstacle.RegisteredBy != User.Identity?.Name)
            {
                return Forbid();
            }

            // Update only the fields that were missing
            existingObstacle.ObstacleName = obstacleData.ObstacleName;
            existingObstacle.ObstacleHeight = obstacleData.ObstacleHeight;
            existingObstacle.ObstacleDescription = obstacleData.ObstacleDescription;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect to overview page
            TempData["IsNewRegistration"] = true;
            return RedirectToAction("Overview", new { id = existingObstacle.Id });
        }

        // GET: /Obstacle/DeleteQuickRegister/5
        // Deletes a quick-registered obstacle
        [HttpGet]
        public async Task<IActionResult> DeleteQuickRegister(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            // Security check
            if (obstacle.RegisteredBy != User.Identity?.Name)
            {
                return Forbid();
            }

            // Remove from database
            _context.Obstacles.Remove(obstacle);
            await _context.SaveChangesAsync();

            // Redirect back to quick registered items
            TempData["Message"] = "Obstacle deleted successfully";
            return RedirectToAction("QuickRegisteredItems");
        }

        // GET: /Obstacle/FullRegister
        // Shows the full registration page with map and form
        [HttpGet]
        public IActionResult FullRegister()
        {
            return View(new ObstacleData());
        }

        // POST: /Obstacle/SaveFullRegister
        // Saves a full registration with all details
        [HttpPost]
        public async Task<IActionResult> SaveFullRegister(ObstacleData obstacleData)
        {
            // Check if the data model is valid (validation)
            if (!ModelState.IsValid)
            {
                return View("FullRegister", obstacleData);
            }

            // Automatically capture who registered this obstacle
            obstacleData.RegisteredBy = User.Identity?.Name ?? "Unknown";

            // Set the registration date to now
            obstacleData.RegisteredDate = DateTime.Now;

            // Add the new obstacle to the database
            _context.Obstacles.Add(obstacleData);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // After successful save, redirect to the Overview page
            TempData["IsNewRegistration"] = true;
            return RedirectToAction("Overview", new { id = obstacleData.Id });
        }

        // ============================================================
        // OLD METHODS - Kept for backward compatibility
        // ============================================================

        // GET: /Obstacle/Workspace
        // This is the old workspace - now replaced by RegisterType + FullRegister
        [HttpGet]
        public IActionResult Workspace()
        {
            // Redirect to the new registration type selection
            return RedirectToAction("RegisterType");
        }

        // POST: /Obstacle/SaveObstacle
        // Old method - redirects to new flow
        [HttpPost]
        public async Task<IActionResult> SaveObstacle(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
            {
                return View("FullRegister", obstacleData);
            }

            obstacleData.RegisteredBy = User.Identity?.Name ?? "Unknown";
            obstacleData.RegisteredDate = DateTime.Now;

            _context.Obstacles.Add(obstacleData);
            await _context.SaveChangesAsync();

            TempData["IsNewRegistration"] = true;
            return RedirectToAction("Overview", new { id = obstacleData.Id });
        }

        // OLD METHOD: Shows the form for registration (GET)
        [HttpGet]
        public IActionResult DataForm()
        {
            // Redirect to new registration flow
            return RedirectToAction("RegisterType");
        }

        // OLD METHOD: Handles form submission (POST)
        [HttpPost]
        public async Task<IActionResult> DataForm(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
            {
                return View(obstacleData);
            }

            obstacleData.RegisteredBy = User.Identity?.Name ?? "Unknown";

            _context.Obstacles.Add(obstacleData);
            await _context.SaveChangesAsync();

            TempData["IsNewRegistration"] = true;
            return RedirectToAction("Overview", new { id = obstacleData.Id });
        }

        // Shows Overview (works for both new and existing obstacles)
        [HttpGet]
        public async Task<IActionResult> Overview(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            ViewBag.IsNewRegistration = TempData["IsNewRegistration"] as bool? ?? false;

            return View(obstacle);
        }

        // Shows list of all obstacles
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var obstacles = await _context.Obstacles.ToListAsync();
            return View(obstacles);
        }
    }

    // Helper class for receiving quick register data from JavaScript
    public class QuickRegisterRequest
    {
        public string ObstacleGeometry { get; set; } = string.Empty;
    }
}