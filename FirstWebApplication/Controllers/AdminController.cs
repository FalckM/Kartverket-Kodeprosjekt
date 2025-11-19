using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using FirstWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserRoleService _roleService;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            UserRoleService roleService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleService = roleService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalObstacles = await _context.Obstacles.CountAsync();

            // Count obstacles by status
            var approvedObstacles = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 3) // Approved
                .Select(s => s.ObstacleId)
                .Distinct()
                .CountAsync();

            var pendingObstacles = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 2) // Pending
                .Select(s => s.ObstacleId)
                .Distinct()
                .CountAsync();

            var rejectedObstacles = await _context.ObstacleStatuses
                .Where(s => s.IsActive && s.StatusTypeId == 4) // Rejected
                .Select(s => s.ObstacleId)
                .Distinct()
                .CountAsync();

            // Count users by role
            var pilots = await _roleService.GetUsersInRoleAsync("Pilot");
            var registerforers = await _roleService.GetUsersInRoleAsync("Registerfører");
            var admins = await _roleService.GetUsersInRoleAsync("Admin");

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalObstacles = totalObstacles;
            ViewBag.ApprovedObstacles = approvedObstacles;
            ViewBag.PendingObstacles = pendingObstacles;
            ViewBag.RejectedObstacles = rejectedObstacles;
            ViewBag.PilotCount = pilots.Count;
            ViewBag.RegisterforerCount = registerforers.Count;
            ViewBag.AdminCount = admins.Count;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AdminUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> AdminManageUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleService.GetAllRolesAsync();

            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Role '{roleName}' assigned to {user.Email}";
            }
            else
            {
                TempData["Error"] = "Failed to assign role";
            }

            return RedirectToAction("AdminManageUser", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (roleName == "Admin")
            {
                var admins = await _roleService.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Cannot remove the last Admin user!";
                    return RedirectToAction("AdminManageUser", new { id = userId });
                }
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Role '{roleName}' removed from {user.Email}";
            }
            else
            {
                TempData["Error"] = "Failed to remove role";
            }

            return RedirectToAction("AdminManageUser", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (user.Email == User.Identity?.Name)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction("AdminUsers");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
            {
                var admins = await _roleService.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Cannot delete the last Admin user!";
                    return RedirectToAction("AdminUsers");
                }
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["Message"] = $"User {user.Email} has been deleted";
            }
            else
            {
                TempData["Error"] = "Failed to delete user";
            }

            return RedirectToAction("AdminUsers");
        }

        [HttpGet]
        public async Task<IActionResult> AdminStatistics()
        {
            var oneWeekAgo = DateTime.Now.AddDays(-7);

            // Obstacle statistics using new structure
            var obstacleStats = new
            {
                Total = await _context.Obstacles.CountAsync(),
                Approved = await _context.ObstacleStatuses
                    .Where(s => s.IsActive && s.StatusTypeId == 3)
                    .Select(s => s.ObstacleId)
                    .Distinct()
                    .CountAsync(),
                Pending = await _context.ObstacleStatuses
                    .Where(s => s.IsActive && s.StatusTypeId == 2)
                    .Select(s => s.ObstacleId)
                    .Distinct()
                    .CountAsync(),
                Rejected = await _context.ObstacleStatuses
                    .Where(s => s.IsActive && s.StatusTypeId == 4)
                    .Select(s => s.ObstacleId)
                    .Distinct()
                    .CountAsync(),
                ThisWeek = await _context.Obstacles
                    .Where(o => o.RegisteredDate >= oneWeekAgo)
                    .CountAsync()
            };

            // User statistics
            var pilots = await _roleService.GetUsersInRoleAsync("Pilot");
            var registerforers = await _roleService.GetUsersInRoleAsync("Registerfører");
            var admins = await _roleService.GetUsersInRoleAsync("Admin");

            ViewBag.ObstacleStats = obstacleStats;
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.PilotCount = pilots.Count;
            ViewBag.RegisterforerCount = registerforers.Count;
            ViewBag.AdminCount = admins.Count;

            return View();
        }
    }
}