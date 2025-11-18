using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    // Only users with "Registerfører" role can access this controller
    [Authorize(Roles = "Registerfører")]
    public class RegisterforerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterforerController> _logger;

        public RegisterforerController(ApplicationDbContext context, ILogger<RegisterforerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Registerforer/RegisterforerDashboard
        // Landing page for Registerfører after login
        [HttpGet]
        public async Task<IActionResult> RegisterforerDashboard()
        {
            // Get counts for dashboard - only count completed obstacles
            var pendingCount = await _context.Obstacles.CountAsync(o =>
                !o.IsApproved && !o.IsRejected
                && !string.IsNullOrEmpty(o.ObstacleName)
                && o.ObstacleHeight > 0
                && !string.IsNullOrEmpty(o.ObstacleDescription));

            var approvedCount = await _context.Obstacles.CountAsync(o => o.IsApproved);
            var rejectedCount = await _context.Obstacles.CountAsync(o => o.IsRejected);

            ViewBag.PendingCount = pendingCount;
            ViewBag.ApprovedCount = approvedCount;
            ViewBag.RejectedCount = rejectedCount;

            return View();
        }

        // GET: /Registerforer/PendingObstacles
        // Show all obstacles waiting for approval
        [HttpGet]
        public async Task<IActionResult> PendingObstacles()
        {
            // Only show completed obstacles (those with name, height, and description)
            // Incomplete quick registrations should not appear here
            var pendingObstacles = await _context.Obstacles
                .Where(o => !o.IsApproved && !o.IsRejected
                    && !string.IsNullOrEmpty(o.ObstacleName)  // Must have name
                    && o.ObstacleHeight > 0                    // Must have height
                    && !string.IsNullOrEmpty(o.ObstacleDescription)) // Must have description
                .OrderBy(o => o.RegisteredDate)
                .ToListAsync();

            return View(pendingObstacles);
        }

        // GET: /Registerforer/ApprovedObstacles
        // Show all approved obstacles
        [HttpGet]
        public async Task<IActionResult> ApprovedObstacles()
        {
            var approvedObstacles = await _context.Obstacles
                .Where(o => o.IsApproved)
                .OrderByDescending(o => o.ApprovedDate)
                .ToListAsync();

            return View(approvedObstacles);
        }

        // GET: /Registerforer/RejectedObstacles
        // Show all rejected obstacles
        [HttpGet]
        public async Task<IActionResult> RejectedObstacles()
        {
            var rejectedObstacles = await _context.Obstacles
                .Where(o => o.IsRejected)
                .OrderByDescending(o => o.RejectedDate)
                .ToListAsync();

            return View(rejectedObstacles);
        }

        // GET: /Registerforer/ReviewObstacle/5
        // Review a specific obstacle
        [HttpGet]
        public async Task<IActionResult> ReviewObstacle(int? id)
        {
            if (id == null)
                return NotFound();

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
                return NotFound();

            return View(obstacle);
        }

        // POST: /Registerforer/ApproveObstacle/5
        [HttpPost]
        public async Task<IActionResult> ApproveObstacle(int id, string approvalComments)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
                return NotFound();

            obstacle.IsApproved = true;
            obstacle.IsRejected = false;
            obstacle.ApprovedBy = User.Identity?.Name;
            obstacle.ApprovedDate = DateTime.Now;
            obstacle.ApprovalComments = approvalComments;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Obstacle '{obstacle.ObstacleName}' has been approved!";
            return RedirectToAction("PendingObstacles");
        }

        // POST: /Registerforer/RejectObstacle/5
        [HttpPost]
        public async Task<IActionResult> RejectObstacle(int id, string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["Error"] = "Rejection reason is required!";
                return RedirectToAction("ReviewObstacle", new { id });
            }

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
                return NotFound();

            obstacle.IsRejected = true;
            obstacle.IsApproved = false;
            obstacle.RejectedBy = User.Identity?.Name;
            obstacle.RejectedDate = DateTime.Now;
            obstacle.RejectionReason = rejectionReason;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Obstacle '{obstacle.ObstacleName}' has been rejected.";
            return RedirectToAction("PendingObstacles");
        }

        // GET: /Registerforer/ViewObstacle/5
        // View details of any obstacle (approved or rejected)
        [HttpGet]
        public async Task<IActionResult> ViewObstacle(int? id)
        {
            if (id == null)
                return NotFound();

            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
                return NotFound();

            return View(obstacle);
        }

        /// API endpoint for Map View - returnerer alle obstacles med filter-muligheter
        /// Brukes av Registerfører for å se alle obstacles på kartet
  
        /// </summary>
        /// <param name="type">Filter på obstacle type (f.eks. "Tower", "Power Line")</param>
        /// <param name="status">Filter på status: "approved", "pending", "rejected", eller "all"</param>
        [HttpGet]
        public async Task<IActionResult> GetObstaclesForMapView(string? type = null, string? status = null)
        {
            try
            {
                // Start med å hente alle obstacles
                var query = _context.Obstacles.AsQueryable();

                // FILTER 1: Filter på ObstacleType hvis spesifisert
                // Eksempel: Hvis type = "Tower", vis bare tårn
                if (!string.IsNullOrEmpty(type) && type != "all")
                {
                    query = query.Where(o => o.ObstacleType == type);
                }

                // FILTER 2: Filter på status hvis spesifisert
                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    switch (status.ToLower())
                    {
                        case "approved":
                            // Bare godkjente obstacles
                            query = query.Where(o => o.IsApproved);
                            break;
                        case "pending":
                            // Bare obstacles som venter på godkjenning
                            // (har navn og høyde, men ikke godkjent eller avvist)
                            query = query.Where(o => !o.IsApproved && !o.IsRejected
                                                   && !string.IsNullOrEmpty(o.ObstacleName)
                                                   && o.ObstacleHeight > 0);
                            break;
                        case "rejected":
                            // Bare avviste obstacles
                            query = query.Where(o => o.IsRejected);
                            break;
                    }
                }

                // Hent dataene fra databasen
                var obstacles = await query
                    .Select(o => new
                    {
                        // Grunnleggende info
                        id = o.Id,
                        name = o.ObstacleName ?? "Unnamed",
                        type = o.ObstacleType ?? "Unknown",
                        height = o.ObstacleHeight,
                        description = o.ObstacleDescription ?? "",

                        // Geometri for å vise på kart
                        geometry = o.ObstacleGeometry,

                        // Status-informasjon
                        isApproved = o.IsApproved,
                        isRejected = o.IsRejected,
                        isPending = !o.IsApproved && !o.IsRejected
                                  && !string.IsNullOrEmpty(o.ObstacleName)
                                  && o.ObstacleHeight > 0,

                        // Godkjenning/avvisning info
                        approvedBy = o.ApprovedBy ?? "",
                        approvedDate = o.ApprovedDate,
                        rejectedBy = o.RejectedBy ?? "",
                        rejectedDate = o.RejectedDate,
                        rejectionReason = o.RejectionReason ?? "",

                        // Registrerings-info (anonymisert for piloter, men Registerfører kan se)
                        registeredBy = o.RegisteredBy ?? "Unknown",
                        registeredDate = o.RegisteredDate
                    })
                    .OrderByDescending(o => o.registeredDate)
                    .ToListAsync();

                // Tell opp statistikk
                var stats = new
                {
                    total = obstacles.Count,
                    approved = obstacles.Count(o => o.isApproved),
                    pending = obstacles.Count(o => o.isPending),
                    rejected = obstacles.Count(o => o.isRejected)
                };

                // Returner både obstacles og statistikk
                return Json(new
                {
                    obstacles = obstacles,
                    stats = stats
                });
            }
            catch (Exception ex)
            {
                // Logger feilen
                Console.WriteLine($"Error in GetObstaclesForMapView: {ex.Message}");
                return StatusCode(500, new { error = "Failed to load obstacles" });
            }
        }

        [HttpGet]
        public IActionResult MapView()
        {
            return View();
        }
    }
}