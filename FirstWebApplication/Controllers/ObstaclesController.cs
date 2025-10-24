using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication.Data;
using FirstWebApplication.Models;
/*
namespace FirstWebApplication.Controllers
{
    public class ObstaclesController : Controller
    {
        private readonly AppDbContext _context;

        public ObstaclesController(AppDbContext context)
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
        public IActionResult DataForm(ObstacleData obstacle)
        {
            if (ModelState.IsValid)
            {
                _context.Obstacles.Add(obstacle);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Obstacle registered successfully!";
                return RedirectToAction("DataForm"); // or redirect to a list page
            }

            // If validation fails, return to the form with validation messages
            return View(obstacle);
        }
    }
}*/