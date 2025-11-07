using FirstWebApplication.Data;
using FirstWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    // Only users with "Admin" role can access this controller
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserRoleService _roleService;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<IdentityUser> userManager,
            UserRoleService roleService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleService = roleService;
            _context = context;
        }

        // GET: /Admin/AdminDashboard
        // Admin dashboard with statistics
        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            // Get statistics
            var totalUsers = await _userManager.Users.CountAsync();
            var totalObstacles = await _context.Obstacles.CountAsync();
            var approvedObstacles = await _context.Obstacles.CountAsync(o => o.IsApproved);
            var pendingObstacles = await _context.Obstacles.CountAsync(o => !o.IsApproved && !o.IsRejected);
            var rejectedObstacles = await _context.Obstacles.CountAsync(o => o.IsRejected);

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

        // GET: /Admin/AdminUsers
        // List all users
        [HttpGet]
        public async Task<IActionResult> AdminUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            // Get roles for each user
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        // GET: /Admin/AdminManageUser/user-id
        // Manage a specific user's roles
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

        // POST: /Admin/AssignRole
        // Assign a role to a user
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

        // POST: /Admin/RemoveRole
        // Remove a role from a user
        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Don't allow removing the last Admin
            if (roleName == "Admin")
            {
                var admins = await _roleService.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Cannot remove the last Admin user!";
                    return RedirectToAction("ManageUser", new { id = userId });
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

        // POST: /Admin/DeleteUser
        // Delete a user account
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Don't allow deleting yourself
            if (user.Email == User.Identity?.Name)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction("Users");
            }

            // Don't allow deleting the last Admin
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

        // GET: /Admin/AdminStatistics
        // Detailed statistics page
        [HttpGet]
        public async Task<IActionResult> AdminStatistics()
        {
            // Obstacle statistics
            var obstacleStats = new
            {
                Total = await _context.Obstacles.CountAsync(),
                Approved = await _context.Obstacles.CountAsync(o => o.IsApproved),
                Pending = await _context.Obstacles.CountAsync(o => !o.IsApproved && !o.IsRejected),
                Rejected = await _context.Obstacles.CountAsync(o => o.IsRejected),
                ThisWeek = await _context.Obstacles.CountAsync(o => o.RegisteredDate >= DateTime.Now.AddDays(-7)),
                ThisMonth = await _context.Obstacles.CountAsync(o => o.RegisteredDate >= DateTime.Now.AddMonths(-1))
            };

            // User statistics
            var pilots = await _roleService.GetUsersInRoleAsync("Pilot");
            var registerforers = await _roleService.GetUsersInRoleAsync("Registerfører");
            var admins = await _roleService.GetUsersInRoleAsync("Admin");

            ViewBag.ObstacleStats = obstacleStats;
            ViewBag.PilotCount = pilots.Count;
            ViewBag.RegisterforerCount = registerforers.Count;
            ViewBag.AdminCount = admins.Count;
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();

            return View();
        }
    }
}