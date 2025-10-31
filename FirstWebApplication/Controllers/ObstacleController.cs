using FirstWebApplication.Models;
using FirstWebApplication.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: /Obstacle/Workspace
        // This is the main workspace where users can register obstacles
        // Shows an interactive map with a sidebar form
        [HttpGet]
        public IActionResult Workspace()
        {
            // Create a new empty ObstacleData model for the form
            return View(new ObstacleData());
        }

        // POST: /Obstacle/SaveObstacle
        // This handles the form submission from the Workspace
        [HttpPost]
        public async Task<IActionResult> SaveObstacle(ObstacleData obstacleData)
        {
            // Check if the data model is valid (validation)
            if (!ModelState.IsValid)
            {
                // If validation fails, show the workspace again with error messages
                return View("Workspace", obstacleData);
            }

            // AUTOMATICALLY CAPTURE WHO REGISTERED THIS OBSTACLE! 
            // User.Identity.Name contains the logged-in user's email
            obstacleData.RegisteredBy = User.Identity?.Name ?? "Unknown";

            // Set the registration date to now
            obstacleData.RegisteredDate = DateTime.Now;

            // Add the new obstacle to the database
            _context.Obstacles.Add(obstacleData);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // After successful save, redirect to the Overview page
            // This shows the user what they just registered
            TempData["IsNewRegistration"] = true;
            return RedirectToAction("Overview", new { id = obstacleData.Id });
        }

        // OLD METHOD: Shows the form for registration (GET)
        // This is kept for backward compatibility with existing links
        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        // OLD METHOD: Handles form submission (POST)
        // This is kept for backward compatibility
        [HttpPost]
        public async Task<IActionResult> DataForm(ObstacleData obstacleData)
        {
            // Checks if the data model is valid (validation)
            if (!ModelState.IsValid)
            {
                return View(obstacleData);
            }

            //  AUTOMATICALLY CAPTURE WHO REGISTERED THIS OBSTACLE! 
            obstacleData.RegisteredBy = User.Identity?.Name ?? "Unknown";

            // Adds the new obstacle to the database
            _context.Obstacles.Add(obstacleData);

            // SaveChangesAsync() writes changes to the database
            await _context.SaveChangesAsync();

            // TempData survives a redirect (unlike ViewBag)
            TempData["IsNewRegistration"] = true;

            // Redirects user to the Overview page with the new ID
            return RedirectToAction("Overview", new { id = obstacleData.Id });
        }

        // Shows Overview (works for both new and existing obstacles)
        [HttpGet]
        public async Task<IActionResult> Overview(int? id)
        {
            // Checks if ID is null or invalid
            if (id == null)
            {
                return NotFound();
            }

            // FindAsync() searches for an obstacle with the specific ID
            var obstacle = await _context.Obstacles.FindAsync(id);

            // If the obstacle doesn't exist, return 404 Not Found
            if (obstacle == null)
            {
                return NotFound();
            }

            // Sets ViewBag based on whether it's a new registration or not
            // TempData is deleted after being read once
            ViewBag.IsNewRegistration = TempData["IsNewRegistration"] as bool? ?? false;

            // Sends the obstacle to the Overview view
            return View(obstacle);
        }

        // Shows list of all obstacles
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ToListAsync() fetches all obstacles from the database as a list
            var obstacles = await _context.Obstacles.ToListAsync();
            return View(obstacles);
        }
    }
}