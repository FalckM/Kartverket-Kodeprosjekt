using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NRLWebApp.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NRLWebApp.Data 
{
    public static class DataSeeder
    {
        
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedRolesAsync(roleManager);
            await SeedStatuserAsync(context);
            await SeedOrganisasjonerAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Pilot", "Registerfører", "Admin" }; 
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedStatuserAsync(ApplicationDbContext context)
        {
            if (!context.Statuser.Any())
            {
                context.Statuser.AddRange(
                    new Status { Navn = "Ny" },
                    new Status { Navn = "Under behandling" },
                    new Status { Navn = "Godkjent" },
                    new Status { Navn = "Avvist" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedOrganisasjonerAsync(ApplicationDbContext context)
        {
            if (!context.Organisasjoner.Any())
            {
                context.Organisasjoner.AddRange(
                    new Organisasjon { Navn = "Norsk Luftambulanse" },
                    new Organisasjon { Navn = "Luftforsvaret" },
                    new Organisasjon { Navn = "Politiets helikoptertjeneste" },
                    new Organisasjon { Navn = "Kartverket" } 
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
