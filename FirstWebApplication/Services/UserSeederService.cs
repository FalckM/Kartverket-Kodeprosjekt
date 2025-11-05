using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Services
{
    /// <summary>
    /// Seeds test users for development
    /// This creates the same test accounts on every developer's computer
    /// </summary>
    public class UserSeederService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserSeederService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Creates test users if they don't exist
        /// Creates one Pilot, one Registerfører, and one Admin
        /// </summary>
        public async Task SeedTestUsersAsync()
        {
            // Test users to create
            var testUsers = new[]
            {
                new { Email = "pilot@test.com", Password = "Pilot123", Role = "Pilot" },
                new { Email = "registerforer@test.com", Password = "Register123", Role = "Registerfører" },
                new { Email = "admin@test.com", Password = "Admin123", Role = "Admin" }
            };

            foreach (var testUser in testUsers)
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(testUser.Email);
                
                if (existingUser == null)
                {
                    // Create the user
                    var user = new IdentityUser
                    {
                        UserName = testUser.Email,
                        Email = testUser.Email,
                        EmailConfirmed = true // Skip email confirmation for test users
                    };

                    var result = await _userManager.CreateAsync(user, testUser.Password);

                    if (result.Succeeded)
                    {
                        // Assign the role
                        await _userManager.AddToRoleAsync(user, testUser.Role);
                    }
                }
            }
        }
    }
}
