using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Services
{
    /// <summary>
    /// Service to manage user roles using ASP.NET Core Identity
    /// </summary>
    public class UserRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Creates a new role in the system
        /// </summary>
        public async Task<bool> CreateRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                return result.Succeeded;
            }
            return false;
        }

        /// <summary>
        /// Assigns a role to a user by email
        /// </summary>
        public async Task<bool> AssignRoleToUserAsync(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await CreateRoleAsync(roleName);
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        /// <summary>
        /// Removes a role from a user
        /// </summary>
        public async Task<bool> RemoveRoleFromUserAsync(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        /// <summary>
        /// Gets all roles assigned to a user
        /// </summary>
        public async Task<IList<string>> GetUserRolesAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return new List<string>();
            }

            return await _userManager.GetRolesAsync(user);
        }

        /// <summary>
        /// Checks if a user has a specific role
        /// </summary>
        public async Task<bool> IsUserInRoleAsync(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        /// <summary>
        /// Gets all roles in the system
        /// </summary>
        public async Task<List<string>> GetAllRolesAsync()
        {
            var roles = await Task.Run(() => _roleManager.Roles.Select(r => r.Name).ToList());
            return roles.Where(r => r != null).Cast<string>().ToList();
        }

        /// <summary>
        /// Gets all users with a specific role
        /// </summary>
        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }
    }
}