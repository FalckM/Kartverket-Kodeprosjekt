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
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>(); 

            await SeedRolesAsync(roleManager);
            await SeedStatuserAsync(context);
            await SeedOrganisasjonerAsync(context);
            await SeedHinderTyperAsync(context);
            await SeedUsersAsync(context, userManager); 
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

        private static async Task SeedHinderTyperAsync(ApplicationDbContext context)
        {
            if (!context.HinderTyper.Any())
            {
                context.HinderTyper.AddRange(
                    new HinderType { Navn = "Mast", Beskrivelse = "Mast, stolpe, tårn eller lignende vertikal struktur." },
                    new HinderType { Navn = "Linjespenn", Beskrivelse = "Strømledning, kabel, vaier eller lignende spent mellom to punkter." },
                    new HinderType { Navn = "Bygning", Beskrivelse = "Byggverk, hus eller annen større struktur." },
                    new HinderType { Navn = "Kran", Beskrivelse = "Byggekran, mobilkran eller lignende." },
                    new HinderType { Navn = "Annet", Beskrivelse = "Andre typer hindre som ikke passer de øvrige kategoriene." }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedUsersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            string pilotEmail = "pilot@test.no";
            if (await userManager.FindByEmailAsync(pilotEmail) == null)
            {
                var org = await context.Organisasjoner.FirstOrDefaultAsync(o => o.Navn == "Norsk Luftambulanse");

                var user = new ApplicationUser
                {
                    UserName = pilotEmail,
                    Email = pilotEmail,
                    Fornavn = "Pilot",
                    Etternavn = "Testbruker",
                    OrganisasjonID = (org != null) ? org.OrganisasjonID : 1, 
                    EmailConfirmed = true 
                };

                var result = await userManager.CreateAsync(user, "Passord!23");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Pilot");
                }
            }

            string regEmail = "registerforer@test.no";
            if (await userManager.FindByEmailAsync(regEmail) == null)
            {
                var org = await context.Organisasjoner.FirstOrDefaultAsync(o => o.Navn == "Kartverket");

                var user = new ApplicationUser
                {
                    UserName = regEmail,
                    Email = regEmail,
                    Fornavn = "Registerfører",
                    Etternavn = "Testbruker",
                    OrganisasjonID = (org != null) ? org.OrganisasjonID : 1,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Passord!23");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Registerfører");
                }
            }

            string adminEmail = "admin@test.no";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var org = await context.Organisasjoner.FirstOrDefaultAsync(o => o.Navn == "Kartverket");

                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Fornavn = "Admin",
                    Etternavn = "Testbruker",
                    OrganisasjonID = (org != null) ? org.OrganisasjonID : 1,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Passord!23");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
