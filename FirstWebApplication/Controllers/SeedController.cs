using FirstWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for å seede testdata.
    /// Kun tilgjengelig for Admin-brukere.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SeedController : Controller
    {
        private readonly DatabaseSeeder _seeder;

        public SeedController(DatabaseSeeder seeder)
        {
            _seeder = seeder;
        }

        /// <summary>
        /// Viser en side hvor admin kan kjøre seeding.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Kjører database seeding.
        /// NB: Kun kjør dette EN gang!
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                await _seeder.SeedAllDataAsync();
                TempData["Success"] = "✅ Database seeding fullført! 20 piloter, 8 obstacle types, og 150 hindringer ble opprettet.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Feil under seeding: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}