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
        public IActionResult Oversikt()
        {
            var userEmail = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            List<ObstacleData> obstacles;

            if (isAdmin)
            {
                obstacles = _context.Obstacles.ToList();
            }
            else {
                // piloter ser bare sitt eget registeredby
                obstacles = _context.Obstacles
                    .Where(o => o.RegisteredBy == userEmail)
                    .ToList();
                }

            //var obstacles = _context.Obstacles.AsQueryable();



            return View(obstacles);
        }

        [HttpPost]
        public IActionResult Edit(ObstacleData updatedObstacle)
        {
            var existing = _context.Obstacles.FirstOrDefault(o => o.Id == updatedObstacle.Id);
            if (existing == null)
                return NotFound();

            var userEmail = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");
            if (existing.RegisteredBy != userEmail && !isAdmin)
                return Forbid();

            existing.ObstacleName = updatedObstacle.ObstacleName;
            existing.ObstacleHeight = updatedObstacle.ObstacleHeight;
            existing.ObstacleDescription = updatedObstacle.ObstacleDescription;
            existing.ObstacleGeometry = updatedObstacle.ObstacleGeometry;
            existing.RegisteredDate = updatedObstacle.RegisteredDate;
            existing.RegisteredBy = updatedObstacle.RegisteredBy;
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult Delete(int id) 
        { 
            var obstacle = _context.Obstacles.FirstOrDefault(o=> o.Id == id);
            if (obstacle == null) return NotFound();

            if (!User.IsInRole("Admin"))
                return Forbid();

            _context.Obstacles.Remove(obstacle);
            _context.SaveChanges();

            return Ok();
        }


    }
}