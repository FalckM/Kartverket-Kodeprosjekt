using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using FirstWebApplication.Models.Obstacle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        // ============================================================
        // REGISTER TYPE - Velg Quick eller Full Register
        // ============================================================
        [HttpGet]
        public IActionResult RegisterType()
        {
            return View();
        }

        // ============================================================
        // QUICK REGISTER - Lagre bare GPS-posisjon
        // ============================================================
        [HttpGet]
        public IActionResult QuickRegister()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> QuickRegister(string obstacleGeometry)
        {
            if (string.IsNullOrEmpty(obstacleGeometry))
            {
                TempData["ErrorMessage"] = "Please mark the obstacle location on the map";
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Opprett nytt Obstacle med bare location (status = Registered)
            var obstacle = new Obstacle
            {
                Location = obstacleGeometry,
                RegisteredByUserId = userId!,
                RegisteredDate = DateTime.Now
            };

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // Opprett første status: Registered (incomplete)
            var status = new ObstacleStatus
            {
                ObstacleId = obstacle.Id,
                StatusTypeId = 1, // Registered
                ChangedByUserId = userId!,
                ChangedDate = DateTime.Now,
                Comments = "Quick Register - location saved",
                IsActive = true
            };

            _context.ObstacleStatuses.Add(status);
            await _context.SaveChangesAsync();

            // Oppdater obstacle med CurrentStatusId
            obstacle.CurrentStatusId = status.Id;
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Location saved! Please complete the registration.";
            return RedirectToAction("RegisterType");
        }

        // ============================================================
        // FULL REGISTER - Registrer alt på en gang
        // ============================================================
        [HttpGet]
        public IActionResult FullRegister()
        {
            return View(new RegisterObstacleViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> FullRegister(RegisterObstacleViewModel model)
        {
            // Manuell validering
            if (string.IsNullOrWhiteSpace(model.ObstacleName))
            {
                ModelState.AddModelError("ObstacleName", "Obstacle name is required");
            }

            if (!model.ObstacleHeight.HasValue || model.ObstacleHeight <= 0)
            {
                ModelState.AddModelError("ObstacleHeight", "Height must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(model.ObstacleDescription))
            {
                ModelState.AddModelError("ObstacleDescription", "Description is required");
            }

            if (string.IsNullOrWhiteSpace(model.ObstacleGeometry))
            {
                ModelState.AddModelError("ObstacleGeometry", "Please mark the location on the map");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Opprett obstacle med alle felt
            var obstacle = new Obstacle
            {
                Name = model.ObstacleName,
                Height = model.ObstacleHeight,
                Description = model.ObstacleDescription,
                Location = model.ObstacleGeometry,
                RegisteredByUserId = userId!,
                RegisteredDate = DateTime.Now
            };

            // Finn ObstacleType hvis oppgitt
            if (!string.IsNullOrWhiteSpace(model.ObstacleType))
            {
                var obstacleType = await _context.ObstacleTypes
                    .FirstOrDefaultAsync(ot => ot.Name == model.ObstacleType);

                if (obstacleType != null)
                {
                    obstacle.ObstacleTypeId = obstacleType.Id;
                }
            }

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // Opprett status: Pending (venter på godkjenning)
            var status = new ObstacleStatus
            {
                ObstacleId = obstacle.Id,
                StatusTypeId = 2, // Pending
                ChangedByUserId = userId!,
                ChangedDate = DateTime.Now,
                Comments = "Full registration completed",
                IsActive = true
            };

            _context.ObstacleStatuses.Add(status);
            await _context.SaveChangesAsync();

            // Oppdater obstacle med CurrentStatusId
            obstacle.CurrentStatusId = status.Id;
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Obstacle registered successfully!";
            return RedirectToAction("Overview", new { id = obstacle.Id });
        }

        // ============================================================
        // COMPLETE QUICK REGISTER - Fullfør en ufullstendig registrering
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> CompleteQuickRegister(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Hent obstacle med status = Registered (incomplete)
            var obstacle = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s!.StatusType)
                .Include(o => o.RegisteredByUser)
                .Where(o => o.Id == id
                    && o.RegisteredByUserId == userId
                    && o.CurrentStatus!.StatusTypeId == 1) // Registered
                .FirstOrDefaultAsync();

            if (obstacle == null)
            {
                TempData["ErrorMessage"] = "Quick registration not found or already completed.";
                return RedirectToAction("MyRegistrations");
            }

            // Lag ViewModel
            var viewModel = new CompleteQuickRegViewModel
            {
                ObstacleId = obstacle.Id,
                ObstacleGeometry = obstacle.Location,
                RegisteredDate = obstacle.RegisteredDate,
                RegisteredBy = obstacle.RegisteredByUser?.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteQuickRegister(CompleteQuickRegViewModel model)
        {
            // Manuell validering
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
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Hent obstacle
            var obstacle = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                .Where(o => o.Id == model.ObstacleId
                    && o.RegisteredByUserId == userId
                    && o.CurrentStatus!.StatusTypeId == 1) // Registered
                .FirstOrDefaultAsync();

            if (obstacle == null)
            {
                TempData["ErrorMessage"] = "Quick registration not found or already completed.";
                return RedirectToAction("MyRegistrations");
            }

            // Oppdater obstacle med detaljer
            obstacle.Name = model.ObstacleName;
            obstacle.Height = model.ObstacleHeight;
            obstacle.Description = model.ObstacleDescription;

            // Finn ObstacleType hvis oppgitt
            if (!string.IsNullOrWhiteSpace(model.ObstacleType))
            {
                var obstacleType = await _context.ObstacleTypes
                    .FirstOrDefaultAsync(ot => ot.Name == model.ObstacleType);

                if (obstacleType != null)
                {
                    obstacle.ObstacleTypeId = obstacleType.Id;
                }
            }

            _context.Obstacles.Update(obstacle);

            // Deaktiver gammel status
            var oldStatus = obstacle.CurrentStatus;
            if (oldStatus != null)
            {
                oldStatus.IsActive = false;
                _context.ObstacleStatuses.Update(oldStatus);
            }

            // Opprett ny status: Pending
            var newStatus = new ObstacleStatus
            {
                ObstacleId = obstacle.Id,
                StatusTypeId = 2, // Pending
                ChangedByUserId = userId!,
                ChangedDate = DateTime.Now,
                Comments = "Quick registration completed",
                IsActive = true
            };

            _context.ObstacleStatuses.Add(newStatus);
            await _context.SaveChangesAsync();

            // Oppdater CurrentStatusId
            obstacle.CurrentStatusId = newStatus.Id;
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration completed successfully!";
            return RedirectToAction("MyRegistrations");
        }

        // ============================================================
        // MY REGISTRATIONS - Vis alle mine registreringer
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Hent alle obstacles for denne brukeren
            var obstacles = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s!.StatusType)
                .Include(o => o.ObstacleType)
                .Include(o => o.RegisteredByUser)
                .Where(o => o.RegisteredByUserId == userId)
                .OrderByDescending(o => o.RegisteredDate)
                .ToListAsync();

            // Hent siste behandling for hvert obstacle (for å få rejection reason osv.)
            var obstacleIds = obstacles.Select(o => o.Id).ToList();
            var latestBehandlinger = await _context.Behandlinger
                .Include(b => b.RegisterforerUser)
                .Where(b => obstacleIds.Contains(b.ObstacleId))
                .GroupBy(b => b.ObstacleId)
                .Select(g => g.OrderByDescending(b => b.ProcessedDate).First())
                .ToDictionaryAsync(b => b.ObstacleId);

            // Bygg ViewModel
            var viewModel = new MyRegistrationsViewModel();

            foreach (var obstacle in obstacles)
            {
                var statusName = obstacle.CurrentStatus?.StatusType?.Name ?? "Unknown";

                // Incomplete Quick Registrations (Registered status + mangler detaljer)
                if (statusName == "Registered" && string.IsNullOrEmpty(obstacle.Name))
                {
                    viewModel.IncompleteQuickRegs.Add(new IncompleteQuickRegItem
                    {
                        Id = obstacle.Id,
                        Location = obstacle.Location,
                        RegisteredDate = obstacle.RegisteredDate
                    });
                }
                // Pending
                else if (statusName == "Pending")
                {
                    viewModel.PendingObstacles.Add(MapToListItem(obstacle, latestBehandlinger));
                }
                // Approved
                else if (statusName == "Approved")
                {
                    viewModel.ApprovedObstacles.Add(MapToListItem(obstacle, latestBehandlinger));
                }
                // Rejected
                else if (statusName == "Rejected")
                {
                    viewModel.RejectedObstacles.Add(MapToListItem(obstacle, latestBehandlinger));
                }
            }

            return View(viewModel);
        }

        // ============================================================
        // OVERVIEW - Vis detaljer for et obstacle
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Overview(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var obstacle = await _context.Obstacles
                .Include(o => o.ObstacleType)
                .Include(o => o.RegisteredByUser)
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s!.StatusType)
                .Include(o => o.StatusHistory)
                    .ThenInclude(sh => sh.StatusType)
                .Include(o => o.StatusHistory)
                    .ThenInclude(sh => sh.ChangedByUser)
                .Where(o => o.Id == id && o.RegisteredByUserId == userId)
                .FirstOrDefaultAsync();

            if (obstacle == null)
            {
                return NotFound();
            }

            // Hent siste behandling
            var latestBehandling = await _context.Behandlinger
                .Include(b => b.RegisterforerUser)
                .Where(b => b.ObstacleId == id)
                .OrderByDescending(b => b.ProcessedDate)
                .FirstOrDefaultAsync();

            // Bygg ViewModel
            var viewModel = new ObstacleDetailsViewModel
            {
                Id = obstacle.Id,
                Name = obstacle.Name ?? "Unnamed",
                Height = obstacle.Height ?? 0,
                Description = obstacle.Description ?? "No description",
                Type = obstacle.ObstacleType?.Name,
                Location = obstacle.Location,
                RegisteredDate = obstacle.RegisteredDate,
                RegisteredBy = obstacle.RegisteredByUser?.Email ?? "Unknown",
                CurrentStatus = obstacle.CurrentStatus?.StatusType?.Name ?? "Unknown",
                IsPending = obstacle.CurrentStatus?.StatusType?.Name == "Pending",
                IsApproved = obstacle.CurrentStatus?.StatusType?.Name == "Approved",
                IsRejected = obstacle.CurrentStatus?.StatusType?.Name == "Rejected"
            };

            // Legg til behandling-info
            if (latestBehandling != null)
            {
                viewModel.ProcessedBy = latestBehandling.RegisterforerUser?.Email;
                viewModel.ProcessedDate = latestBehandling.ProcessedDate;
                viewModel.ProcessComments = latestBehandling.Comments;
                viewModel.RejectionReason = latestBehandling.RejectionReason;
            }

            // Legg til status history
            if (obstacle.StatusHistory != null)
            {
                viewModel.StatusHistory = obstacle.StatusHistory
                    .OrderBy(sh => sh.ChangedDate)
                    .Select(sh => new StatusHistoryItem
                    {
                        Status = sh.StatusType?.Name ?? "Unknown",
                        ChangedBy = sh.ChangedByUser?.Email ?? "Unknown",
                        ChangedDate = sh.ChangedDate,
                        Comments = sh.Comments
                    })
                    .ToList();
            }

            return View(viewModel);
        }

        // ============================================================
        // DELETE REGISTRATION - Slett et pending obstacle
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> DeleteRegistration(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var obstacle = await _context.Obstacles
                .Include(o => o.CurrentStatus)
                    .ThenInclude(s => s!.StatusType)
                .Where(o => o.Id == id && o.RegisteredByUserId == userId)
                .FirstOrDefaultAsync();

            if (obstacle == null)
            {
                TempData["ErrorMessage"] = "Obstacle not found.";
                return RedirectToAction("MyRegistrations");
            }

            var statusName = obstacle.CurrentStatus?.StatusType?.Name;

            // Kan kun slette Registered eller Pending
            if (statusName != "Registered" && statusName != "Pending")
            {
                TempData["ErrorMessage"] = "You can only delete obstacles that are pending or incomplete.";
                return RedirectToAction("MyRegistrations");
            }

            _context.Obstacles.Remove(obstacle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Obstacle deleted successfully.";
            return RedirectToAction("MyRegistrations");
        }

        // ============================================================
        // HELPER METHOD - Map Obstacle til ObstacleListItemViewModel
        // ============================================================
        private ObstacleListItemViewModel MapToListItem(Obstacle obstacle, Dictionary<long, Behandling> behandlinger)
        {
            var statusName = obstacle.CurrentStatus?.StatusType?.Name ?? "Unknown";

            var item = new ObstacleListItemViewModel
            {
                Id = obstacle.Id,
                Name = obstacle.Name ?? "Unnamed",
                Height = obstacle.Height ?? 0,
                Type = obstacle.ObstacleType?.Name,
                Location = obstacle.Location,
                RegisteredDate = obstacle.RegisteredDate,
                RegisteredBy = obstacle.RegisteredByUser?.Email ?? "Unknown",
                CurrentStatus = statusName,
                IsIncomplete = statusName == "Registered" && string.IsNullOrEmpty(obstacle.Name),
                IsPending = statusName == "Pending",
                IsApproved = statusName == "Approved",
                IsRejected = statusName == "Rejected"
            };

            // Legg til behandling-info hvis finnes
            if (behandlinger.TryGetValue(obstacle.Id, out var behandling))
            {
                item.ProcessedBy = behandling.RegisterforerUser?.Email;
                item.ProcessedDate = behandling.ProcessedDate;
                item.RejectionReason = behandling.RejectionReason;
            }

            return item;
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}