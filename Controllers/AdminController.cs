using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRLWebApp.Data;
using NRLWebApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic; 
using System.Linq; 
using NRLWebApp.Models.ViewModels; 

namespace NRLWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userViewModelList = new List<UserViewModel>();

            var users = await _context.Users
                                .Include(u => u.Organisasjon)
                                .ToListAsync();

            foreach (var user in users)
            {              
                var viewModel = new UserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Fornavn = user.Fornavn,
                    Etternavn = user.Etternavn,
                    OrganisasjonNavn = user.Organisasjon?.Navn ?? "Mangler",
                    Roller = await _userManager.GetRolesAsync(user)
                };

                userViewModelList.Add(viewModel);
            }

            return View(userViewModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return RedirectToAction("Index");
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return RedirectToAction("Index");
            }

            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }

            return RedirectToAction("Index");
        }
    }
}