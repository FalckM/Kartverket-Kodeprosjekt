using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Services
{
    public class RoleInitializerService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleInitializerService> _logger;

        public RoleInitializerService(RoleManager<IdentityRole> roleManager, ILogger<RoleInitializerService> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            string[] roleNames = { "Admin", "Pilot", "Registerfører" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    _logger.LogInformation($"Role '{roleName}' created successfully.");
                }
            }
        }
    }
}