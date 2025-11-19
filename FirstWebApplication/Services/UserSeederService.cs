using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Services
{
    public class UserSeederService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserSeederService> _logger;

        public UserSeederService(UserManager<ApplicationUser> userManager, ILogger<UserSeederService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // Pilot user
            await CreateUserIfNotExists("pilot@test.com", "Pilot123", "Pilot");

            // Registerfører user
            await CreateUserIfNotExists("registerforer@test.com", "Register123", "Registerfører");

            // Admin user
            await CreateUserIfNotExists("admin@test.com", "Admin123", "Admin");
        }

        private async Task CreateUserIfNotExists(string email, string password, string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    _logger.LogInformation($"Test user '{email}' created with role '{role}'.");
                }
                else
                {
                    _logger.LogError($"Failed to create test user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}