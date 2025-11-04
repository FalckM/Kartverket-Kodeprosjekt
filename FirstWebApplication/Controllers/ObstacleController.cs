using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication.Data;
using FirstWebApplication.Models;
using FirstWebApplication.Migrations;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly AppDbContext _context;

        public ObstacleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Obstacles/DataForm
        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        // POST: /Obstacles/DataForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DataForm(ObstacleData obstacleData)
        {
            if (ModelState.IsValid)
            {
                obstacleData.RegisteredBy = User.Identity?.Name;
                _context.Obstacles.Add(obstacleData);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Obstacle registered successfully!";
                return View("Overview", obstacleData);
            }

            // If validation fails, return to the form with validation messages
            return View(obstacleData);
        }

        [HttpGet]
        public IActionResult Oversikt(string? sortOrder)
        {
            var obstacles = _context.Obstacles.AsQueryable();

            // Sortér basert på parameter
            if (sortOrder == "eldst")
            {
                obstacles = obstacles.OrderBy(o => o.RegisteredDate);
            }
            else // standard = nyest først
            {
                obstacles = obstacles.OrderByDescending(o => o.RegisteredDate);
            }

            return View(obstacles.ToList());
        }



    }
}