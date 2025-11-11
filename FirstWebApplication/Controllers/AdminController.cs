using FirstWebApplication.Data;
using FirstWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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
                return RedirectToAction("AdminUsers");
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

        // GET: /Admin/ExportObstaclesToExcel
        // Export all obstacles to Excel file
        [HttpGet]
        public async Task<IActionResult> ExportObstaclesToExcel()
        {
            // Get all obstacles from database, ordered by registration date
            var obstacles = await _context.Obstacles
                .OrderByDescending(o => o.RegisteredDate)
                .ToListAsync();

            // Create a new Excel workbook
            using (var workbook = new XLWorkbook())
            {
                // Add a worksheet
                var worksheet = workbook.Worksheets.Add("Obstacles");

                // --- HEADER ROW (Row 1) ---
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Obstacle Name";
                worksheet.Cell(1, 3).Value = "Height (m)";
                worksheet.Cell(1, 4).Value = "Description";
                worksheet.Cell(1, 5).Value = "Obstacle Type";
                worksheet.Cell(1, 6).Value = "Geometry Type";
                worksheet.Cell(1, 7).Value = "Coordinates";
                worksheet.Cell(1, 8).Value = "Registered By";
                worksheet.Cell(1, 9).Value = "Registration Date";
                worksheet.Cell(1, 10).Value = "Status";
                worksheet.Cell(1, 11).Value = "Approved/Rejected By";
                worksheet.Cell(1, 12).Value = "Approved/Rejected Date";
                worksheet.Cell(1, 13).Value = "Comments/Reason";

                // Style the header row
                var headerRow = worksheet.Range(1, 1, 1, 13);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // --- DATA ROWS (Starting from Row 2) ---
                int currentRow = 2;
                foreach (var obstacle in obstacles)
                {
                    worksheet.Cell(currentRow, 1).Value = obstacle.Id;
                    worksheet.Cell(currentRow, 2).Value = obstacle.ObstacleName ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = obstacle.ObstacleHeight;
                    worksheet.Cell(currentRow, 4).Value = obstacle.ObstacleDescription ?? "N/A";
                    worksheet.Cell(currentRow, 5).Value = obstacle.ObstacleType ?? "N/A";

                    // Parse geometry type from WKT string
                    string geometryType = "Unknown";
                    if (!string.IsNullOrEmpty(obstacle.ObstacleGeometry))
                    {
                        if (obstacle.ObstacleGeometry.StartsWith("POINT"))
                            geometryType = "Point";
                        else if (obstacle.ObstacleGeometry.StartsWith("LINESTRING"))
                            geometryType = "Line";
                        else if (obstacle.ObstacleGeometry.StartsWith("POLYGON"))
                            geometryType = "Polygon";
                    }
                    worksheet.Cell(currentRow, 6).Value = geometryType;
                    worksheet.Cell(currentRow, 7).Value = obstacle.ObstacleGeometry ?? "N/A";

                    worksheet.Cell(currentRow, 8).Value = obstacle.RegisteredBy ?? "N/A";
                    worksheet.Cell(currentRow, 9).Value = obstacle.RegisteredDate.ToString("yyyy-MM-dd HH:mm:ss");

                    // Determine status
                    string status;
                    if (obstacle.IsApproved)
                        status = "Approved";
                    else if (obstacle.IsRejected)
                        status = "Rejected";
                    else if (string.IsNullOrEmpty(obstacle.ObstacleName) || obstacle.ObstacleHeight == 0)
                        status = "Incomplete";
                    else
                        status = "Pending";

                    worksheet.Cell(currentRow, 10).Value = status;

                    // Color-code the status cell
                    var statusCell = worksheet.Cell(currentRow, 10);
                    if (status == "Approved")
                        statusCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    else if (status == "Rejected")
                        statusCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    else if (status == "Pending")
                        statusCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    else if (status == "Incomplete")
                        statusCell.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Cell(currentRow, 11).Value = obstacle.ApprovedBy ?? obstacle.RejectedBy ?? "N/A";
                    worksheet.Cell(currentRow, 12).Value = obstacle.ApprovedDate?.ToString("yyyy-MM-dd HH:mm:ss")
                        ?? obstacle.RejectedDate?.ToString("yyyy-MM-dd HH:mm:ss")
                        ?? "N/A";
                    worksheet.Cell(currentRow, 13).Value = obstacle.ApprovalComments ?? obstacle.RejectionReason ?? "N/A";

                    currentRow++;
                }

                // Auto-fit all columns for better readability
                worksheet.Columns().AdjustToContents();

                // Freeze the header row so it stays visible when scrolling
                worksheet.SheetView.FreezeRows(1);

                // Add filters to the header row
                worksheet.RangeUsed().SetAutoFilter();

                // Create a memory stream to save the Excel file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    // Generate filename with current date
                    string fileName = $"Obstacles_Export_{DateTime.Now:yyyy-MM-dd_HHmmss}.xlsx";

                    // Return the file for download
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
        }
    }
}