using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Services
{
    /// <summary>
    /// Service that creates the three roles (Pilot, Registerfører, Admin) when the app starts
    /// This runs once at application startup to ensure all roles exist
    /// </summary>
    public class RoleInitializerService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleInitializerService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        /// <summary>
        /// Creates the three roles if they don't already exist
        /// </summary>
        public async Task InitializeRolesAsync()
        {
            // Define the three roles for this application
            string[] roleNames = { "Pilot", "Registerfører", "Admin" };

            foreach (var roleName in roleNames)
            {
                // Check if role exists
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                
                if (!roleExist)
                {
                    // Create the role if it doesn't exist
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
