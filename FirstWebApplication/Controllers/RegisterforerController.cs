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
        private readonly ILogger<RegisterforerController> _logger;

        public RegisterforerController(ApplicationDbContext context, ILogger<RegisterforerController> logger)
        {
            _context = context;
            _logger = logger;
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
                .Include(o => o.RegisteredByUser)
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
                RegisteredBy = o.RegisteredByUser?.Email ?? "Unknown",
                RegisteredDate = o.RegisteredDate,
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
                .Include(o => o.ObstacleType)
                .Include(o => o.RegisteredByUser)
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s.ChangedByUser)
                .Where(o => rejectedObstacleIds.Contains(o.Id))
                .ToListAsync();

            var viewModels = obstacles.Select(o => new ObstacleListItemViewModel
            {
                Id = o.Id,
                Name = o.Name ?? "Unnamed",
                Type = o.ObstacleType?.Name,
                Height = o.Height ?? 0,
                RegisteredBy = o.RegisteredByUser?.Email ?? "Unknown",
                RegisteredDate = o.RegisteredDate,
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveObstacle(ApproveObstacleViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("ReviewObstacle", new { id = model.ObstacleId });

            var obstacle = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                .FirstOrDefaultAsync(o => o.Id == model.ObstacleId);

            if (obstacle == null)
                return NotFound();

            if (obstacle.CurrentStatus != null)
            {
                obstacle.CurrentStatus.IsActive = false;
                _context.ObstacleStatuses.Update(obstacle.CurrentStatus);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var newStatus = new ObstacleStatus
            {
                ObstacleId = obstacle.Id,
                StatusTypeId = 3,
                ChangedByUserId = userId,
                ChangedDate = DateTime.Now,
                Comments = model.Comments ?? "",
                IsActive = true
            };

            _context.ObstacleStatuses.Add(newStatus);
            await _context.SaveChangesAsync();

            obstacle.CurrentStatusId = newStatus.Id;
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Obstacle '{obstacle.Name}' has been approved.";
            return RedirectToAction("AllObstacles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectObstacle(RejectObstacleViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("ReviewObstacle", new { id = model.ObstacleId });

            var obstacle = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                .FirstOrDefaultAsync(o => o.Id == model.ObstacleId);

            if (obstacle == null)
                return NotFound();

            if (obstacle.CurrentStatus != null)
            {
                obstacle.CurrentStatus.IsActive = false;
                _context.ObstacleStatuses.Update(obstacle.CurrentStatus);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var newStatus = new ObstacleStatus
            {
                ObstacleId = obstacle.Id,
                StatusTypeId = 4,
                ChangedByUserId = userId,
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
            return RedirectToAction("AllObstacles");
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
        public async Task<IActionResult> AllObstacles(string? status = null, string? sortBy = null, string? sortOrder = "desc")
        {
            try
            {
                _logger.LogInformation("🔍 AllObstacles called - Status: {Status}, SortBy: {SortBy}, SortOrder: {SortOrder}",
                    status, sortBy, sortOrder);

                var query = _context.Obstacles
                    .Include(o => o.ObstacleType)
                    .Include(o => o.RegisteredByUser)
                    .Include(o => o.CurrentStatus)
                        .ThenInclude(s => s.StatusType)
                    .Include(o => o.CurrentStatus)
                        .ThenInclude(s => s.ChangedByUser)
                    .Where(o => o.CurrentStatusId != null) // Kun obstacles med status
                    .Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId != 1) // Exclude incomplete quick registrations
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
                {
                    if (status.ToLower() == "pending")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 2);
                    else if (status.ToLower() == "approved")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 3);
                    else if (status.ToLower() == "rejected")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 4);
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "name" => sortOrder == "asc" ? query.OrderBy(o => o.Name) : query.OrderByDescending(o => o.Name),
                    "type" => sortOrder == "asc" ? query.OrderBy(o => o.ObstacleType!.Name) : query.OrderByDescending(o => o.ObstacleType!.Name),
                    "height" => sortOrder == "asc" ? query.OrderBy(o => o.Height) : query.OrderByDescending(o => o.Height),
                    "status" => sortOrder == "asc" ? query.OrderBy(o => o.CurrentStatus!.StatusTypeId) : query.OrderByDescending(o => o.CurrentStatus!.StatusTypeId),
                    "registereddate" => sortOrder == "asc" ? query.OrderBy(o => o.RegisteredDate) : query.OrderByDescending(o => o.RegisteredDate),
                    _ => query.OrderByDescending(o => o.RegisteredDate) // Default: nyeste først
                };

                var obstacles = await query.ToListAsync();

                var viewModels = obstacles.Select(o => new ObstacleListItemViewModel
                {
                    Id = o.Id,
                    Name = o.Name ?? "Unnamed",
                    Type = o.ObstacleType?.Name,
                    Height = o.Height ?? 0,
                    RegisteredBy = o.RegisteredByUser?.Email ?? "Unknown",
                    RegisteredDate = o.RegisteredDate,
                    ProcessedBy = o.CurrentStatus?.ChangedByUser?.Email,
                    ProcessedDate = o.CurrentStatus?.ChangedDate,
                    StatusName = o.CurrentStatus?.StatusType?.Name ?? "Unknown",
                    IsPending = o.CurrentStatus?.StatusTypeId == 2,
                    IsApproved = o.CurrentStatus?.StatusTypeId == 3,
                    IsRejected = o.CurrentStatus?.StatusTypeId == 4,
                    RejectionReason = o.CurrentStatus?.StatusTypeId == 4 ? o.CurrentStatus?.Comments : null
                }).ToList();

                // Pass filter/sort parameters to view
                ViewBag.CurrentStatus = status ?? "all";
                ViewBag.CurrentSortBy = sortBy ?? "registereddate";
                ViewBag.CurrentSortOrder = sortOrder;

                _logger.LogInformation($"✅ Returning {viewModels.Count} obstacles");

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in AllObstacles: {Message}", ex.Message);
                return View(new List<ObstacleListItemViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetObstaclesForMapView(string? type = null, string? status = null)
        {
            try
            {
                _logger.LogInformation("🔍 GetObstaclesForMapView called - Type: {Type}, Status: {Status}", type, status);

                // Start with obstacles that have CurrentStatus set (viktig!)
                var query = _context.Obstacles
                    .Include(o => o.ObstacleType)
                    .Include(o => o.CurrentStatus)
                        .ThenInclude(s => s.StatusType)
                    .Include(o => o.RegisteredByUser)
                    .Where(o => o.CurrentStatusId != null) // VIKTIG: Kun obstacles med status
                    .AsQueryable();

                // Count before filtering
                var totalCount = await query.CountAsync();
                _logger.LogInformation($"📊 Total obstacles with CurrentStatus: {totalCount}");

                // Filter by type if specified
                if (!string.IsNullOrEmpty(type) && type.ToLower() != "all")
                {
                    query = query.Where(o => o.ObstacleType != null && o.ObstacleType.Name.ToLower() == type.ToLower());
                    var typeCount = await query.CountAsync();
                    _logger.LogInformation($"📊 After type filter '{type}': {typeCount}");
                }

                // Filter by status if specified
                if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
                {
                    if (status.ToLower() == "approved")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 3);
                    else if (status.ToLower() == "pending")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 2);
                    else if (status.ToLower() == "rejected")
                        query = query.Where(o => o.CurrentStatus != null && o.CurrentStatus.StatusTypeId == 4);

                    var statusCount = await query.CountAsync();
                    _logger.LogInformation($"📊 After status filter '{status}': {statusCount}");
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

                _logger.LogInformation($"✅ Returning {obstacles.Count} obstacles");

                // Log first obstacle for debugging
                if (obstacles.Any())
                {
                    var first = obstacles.First();
                    var geomPreview = string.IsNullOrEmpty(first.geometry) ? "NULL" : first.geometry.Substring(0, Math.Min(50, first.geometry.Length));
                    _logger.LogInformation($"🔍 First obstacle: Id={first.id}, Name={first.name}, Type={first.type}, Geometry={geomPreview}");
                }

                var stats = new
                {
                    total = obstacles.Count,
                    approved = obstacles.Count(o => o.isApproved),
                    pending = obstacles.Count(o => o.isPending),
                    rejected = obstacles.Count(o => o.isRejected)
                };

                _logger.LogInformation($"📊 Stats - Total: {stats.total}, Approved: {stats.approved}, Pending: {stats.pending}, Rejected: {stats.rejected}");

                return Json(new
                {
                    obstacles = obstacles,
                    stats = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetObstaclesForMapView: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to load obstacles", details = ex.Message });
            }
        }
    }
}