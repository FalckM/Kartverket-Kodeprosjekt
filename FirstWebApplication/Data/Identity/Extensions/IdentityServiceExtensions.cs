using FirstWebApplication.Data.Identity;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FirstWebApplication.Data.Identity.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddAppIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            return services;
        }
    }
}
