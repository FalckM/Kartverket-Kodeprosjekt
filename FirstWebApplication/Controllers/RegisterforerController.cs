using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using FirstWebApplication.Models.Obstacle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Registerfører")]
    public class RegisterforerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterforerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> RegisterforerDashboard()
        {
            var pendingCount = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 2)
                .Select(s => s.ObstacleId)
                .Distinct()
                .CountAsync();

            var approvedCount = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 3)
                .Select(s => s.ObstacleId)
                .Distinct()
                .CountAsync();

            var rejectedCount = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 4)
                .Select(s => s.ObstacleId)
                .Distinct()
                .CountAsync();

            ViewBag.PendingCount = pendingCount;
            ViewBag.ApprovedCount = approvedCount;
            ViewBag.RejectedCount = rejectedCount;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PendingObstacles()
        {
            var pendingObstacleIds = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 2)
                .Select(s => s.ObstacleId)
                .Distinct()
                .ToListAsync();

            var obstacles = await _context.Obstacles
                .Include(o => o.ObstacleType)
                .Include(o => o.RegisteredByUser)
                .Where(o => pendingObstacleIds.Contains(o.Id))
                .ToListAsync();

            var viewModels = obstacles.Select(o => new ObstacleListItemViewModel
            {
                Id = o.Id,
                Name = o.Name ?? "Unnamed",
                Type = o.ObstacleType?.Name,
                Height = o.Height ?? 0,
                RegisteredBy = o.RegisteredByUser?.Email ?? "Unknown",
                RegisteredDate = o.RegisteredDate
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> ApprovedObstacles()
        {
            var approvedObstacleIds = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 3)
                .Select(s => s.ObstacleId)
                .Distinct()
                .ToListAsync();

            var obstacles = await _context.Obstacles
                .Include(o => o.ObstacleType)
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s.ChangedByUser)
                .Where(o => approvedObstacleIds.Contains(o.Id))
                .ToListAsync();

            var viewModels = obstacles.Select(o => new ObstacleListItemViewModel
            {
                Id = o.Id,
                Name = o.Name ?? "Unnamed",
                Type = o.ObstacleType?.Name,
                Height = o.Height ?? 0,
                ProcessedBy = o.CurrentStatus?.ChangedByUser?.Email,
                ProcessedDate = o.CurrentStatus?.ChangedDate
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> RejectedObstacles()
        {
            var rejectedObstacleIds = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 4)
                .Select(s => s.ObstacleId)
                .Distinct()
                .ToListAsync();

            var obstacles = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s.ChangedByUser)
                .Where(o => rejectedObstacleIds.Contains(o.Id))
                .ToListAsync();

            var viewModels = obstacles.Select(o => new ObstacleListItemViewModel
            {
                Id = o.Id,
                Name = o.Name ?? "Unnamed",
                ProcessedBy = o.CurrentStatus?.ChangedByUser?.Email,
                ProcessedDate = o.CurrentStatus?.ChangedDate,
                RejectionReason = o.CurrentStatus?.Comments
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewObstacle(int? id)
        {
            if (id == null)
                return NotFound();

            var obstacle = await _context.Obstacles
                .Include(o => o.ObstacleType)
                .Include(o => o.RegisteredByUser)
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s.StatusType)
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s.ChangedByUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obstacle == null)
                return NotFound();

            // Load status history separately
            var statusHistory = await _context.ObstacleStatuses
                .Where(s => s.ObstacleId == obstacle.Id && !s.IsActive)
                .Include(s => s.StatusType)
                .Include(s => s.ChangedByUser)
                .OrderByDescending(s => s.ChangedDate)
                .ToListAsync();

            var viewModel = new ObstacleDetailsViewModel
            {
                Id = obstacle.Id,
                Name = obstacle.Name ?? "Unnamed",
                Type = obstacle.ObstacleType?.Name,
                Height = obstacle.Height ?? 0,
                Description = obstacle.Description ?? "",
                Location = obstacle.Location,
                RegisteredBy = obstacle.RegisteredByUser?.Email ?? "Unknown",
                RegisteredDate = obstacle.RegisteredDate,
                IsPending = obstacle.CurrentStatus?.StatusTypeId == 2,
                IsApproved = obstacle.CurrentStatus?.StatusTypeId == 3,
                IsRejected = obstacle.CurrentStatus?.StatusTypeId == 4,
                ProcessedBy = obstacle.CurrentStatus?.ChangedByUser?.Email,
                ProcessedDate = obstacle.CurrentStatus?.ChangedDate,
                ProcessComments = obstacle.CurrentStatus?.Comments,
                RejectionReason = obstacle.CurrentStatus?.StatusTypeId == 4 ? obstacle.CurrentStatus?.Comments : null,
                StatusHistory = statusHistory
                    .Select(s => new StatusHistoryItem
                    {
                        Status = s.StatusType?.Name ?? "Unknown",
                        ChangedBy = s.ChangedByUser?.Email ?? "Unknown",
                        ChangedDate = s.ChangedDate,
                        Comments = s.Comments
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveObstacle(ApproveObstacleViewModel model)
        {
            var obstacle = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                .FirstOrDefaultAsync(o => o.Id == model.ObstacleId);

            if (obstacle == null)
                return NotFound();

            // Deactivate current status
            if (obstacle.CurrentStatus != null)
            {
                obstacle.CurrentStatus.IsActive = false;
            }

            // Create new approved status
            var newStatus = new ObstacleStatus
            {
                ObstacleId = obstacle.Id,
                StatusTypeId = 3, // Approved
                ChangedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "",
                ChangedDate = DateTime.Now,
                Comments = model.Comments,
                IsActive = true
            };

            _context.ObstacleStatuses.Add(newStatus);
            await _context.SaveChangesAsync();

            obstacle.CurrentStatusId = newStatus.Id;
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Obstacle '{obstacle.Name}' has been approved!";
            return RedirectToAction("PendingObstacles");
        }

        [HttpPost]
        public async Task<IActionResult> RejectObstacle(RejectObstacleViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.RejectionReason))
            {
                TempData["ErrorMessage"] = "Rejection reason is required!";
                return RedirectToAction("ReviewObstacle", new { id = model.ObstacleId });
            }

            var obstacle = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                .FirstOrDefaultAsync(o => o.Id == model.ObstacleId);

            if (obstacle == null)
                return NotFound();

            // Deactivate current status
            if (obstacle.CurrentStatus != null)
            {
                obstacle.CurrentStatus.IsActive = false;
            }

            // Create new rejected status
            var newStatus = new ObstacleStatus
            {
                ObstacleId = obstacle.Id,
                StatusTypeId = 4, // Rejected
                ChangedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "",
                ChangedDate = DateTime.Now,
                Comments = model.RejectionReason + (string.IsNullOrEmpty(model.Comments) ? "" : $"\n\nAdditional: {model.Comments}"),
                IsActive = true
            };

            _context.ObstacleStatuses.Add(newStatus);
            await _context.SaveChangesAsync();

            obstacle.CurrentStatusId = newStatus.Id;
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Obstacle '{obstacle.Name}' has been rejected.";
            return RedirectToAction("PendingObstacles");
        }

        [HttpGet]
        public async Task<IActionResult> ViewObstacle(int? id)
        {
            if (id == null)
                return NotFound();

            var obstacle = await _context.Obstacles
                .Include(o => o.ObstacleType)
                .Include(o => o.RegisteredByUser)
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s.StatusType)
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s.ChangedByUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obstacle == null)
                return NotFound();

            // Load status history separately
            var statusHistory = await _context.ObstacleStatuses
                .Where(s => s.ObstacleId == obstacle.Id && !s.IsActive)
                .Include(s => s.StatusType)
                .Include(s => s.ChangedByUser)
                .OrderByDescending(s => s.ChangedDate)
                .ToListAsync();

            var viewModel = new ObstacleDetailsViewModel
            {
                Id = obstacle.Id,
                Name = obstacle.Name ?? "Unnamed",
                Type = obstacle.ObstacleType?.Name,
                Height = obstacle.Height ?? 0,
                Description = obstacle.Description ?? "",
                Location = obstacle.Location,
                RegisteredBy = obstacle.RegisteredByUser?.Email ?? "Unknown",
                RegisteredDate = obstacle.RegisteredDate,
                IsPending = obstacle.CurrentStatus?.StatusTypeId == 2,
                IsApproved = obstacle.CurrentStatus?.StatusTypeId == 3,
                IsRejected = obstacle.CurrentStatus?.StatusTypeId == 4,
                ProcessedBy = obstacle.CurrentStatus?.ChangedByUser?.Email,
                ProcessedDate = obstacle.CurrentStatus?.ChangedDate,
                ProcessComments = obstacle.CurrentStatus?.Comments,
                RejectionReason = obstacle.CurrentStatus?.StatusTypeId == 4 ? obstacle.CurrentStatus?.Comments : null,
                StatusHistory = statusHistory
                    .Select(s => new StatusHistoryItem
                    {
                        Status = s.StatusType?.Name ?? "Unknown",
                        ChangedBy = s.ChangedByUser?.Email ?? "Unknown",
                        ChangedDate = s.ChangedDate,
                        Comments = s.Comments
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult MapView()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetObstaclesForMapView(string? type = null, string? status = null)
        {
            try
            {
                var query = _context.Obstacles
                    .Include(o => o.ObstacleType)
                    .Include(o => o.CurrentStatus)
                        .ThenInclude(s => s.StatusType)
                    .Include(o => o.RegisteredByUser)
                    .AsQueryable();

                // Filter by type if specified
                if (!string.IsNullOrEmpty(type) && type != "all")
                {
                    query = query.Where(o => o.ObstacleType != null && o.ObstacleType.Name == type);
                }

                // Filter by status if specified
                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    if (status == "approved")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 3);
                    else if (status == "pending")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 2);
                    else if (status == "rejected")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 4);
                }

                var obstacles = await query
                    .Select(o => new
                    {
                        id = o.Id,
                        name = o.Name ?? "Unnamed",
                        type = o.ObstacleType != null ? o.ObstacleType.Name : "Unknown",
                        height = o.Height ?? 0,
                        description = o.Description ?? "",
                        geometry = o.Location,

                        // Status from CurrentStatus
                        isApproved = o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 3,
                        isRejected = o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 4,
                        isPending = o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 2,

                        statusName = o.CurrentStatus != null && o.CurrentStatus.StatusType != null
                            ? o.CurrentStatus.StatusType.Name
                            : "Unknown",

                        registeredBy = o.RegisteredByUser != null ? o.RegisteredByUser.Email : "Unknown",
                        registeredDate = o.RegisteredDate
                    })
                    .OrderByDescending(o => o.registeredDate)
                    .ToListAsync();

                var stats = new
                {
                    total = obstacles.Count,
                    approved = obstacles.Count(o => o.isApproved),
                    pending = obstacles.Count(o => o.isPending),
                    rejected = obstacles.Count(o => o.isRejected)
                };

                return Json(new
                {
                    obstacles = obstacles,
                    stats = stats
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetObstaclesForMapView: {ex.Message}");
                return StatusCode(500, new { error = "Failed to load obstacles" });
            }
        }
    }
}