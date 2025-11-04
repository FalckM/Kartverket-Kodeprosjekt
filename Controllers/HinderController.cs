using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NRLWebApp.Data;
using NRLWebApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NRLWebApp.Controllers
{
    public class HinderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HinderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Pilot")] 
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Hinder/Create
        // Denne metoden kjører når brukeren sender inn skjemaet
        [HttpPost]
        [ValidateAntiForgeryToken] // Sikkerhetstiltak mot angrep
        [Authorize(Roles = "Pilot")]
        public async Task<IActionResult> Create([Bind("Navn,Hoyde,Beskrivelse,Lokasjon")] Hinder hinder)
        {
            try
            {
                // Vi fjerner feilmeldinger for felter vi vet vi skal sette manuelt.
                // Dette er nødvendig fordi [Bind] kun er for sikkerhet, ikke validering.
                ModelState.Remove("ApplicationUserId");
                ModelState.Remove("StatusID");
                ModelState.Remove("ApplicationUser");
                ModelState.Remove("Status");
                ModelState.Remove("Behandlinger");

                if (ModelState.IsValid)
                {
                    // 1. Finne piloten som er logget inn
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        // Burde ikke skje pga. [Authorize], men god sikkerhet
                        return Challenge();
                    }

                    // 2. Finne StatusID for "Ny" (vi antar at den finnes)
                    var nyStatus = await _context.Statuser.FirstOrDefaultAsync(s => s.Navn == "Ny");
                    if (nyStatus == null)
                    {
                        // Hvis "Ny" status mangler i databasen, er noe galt
                        ModelState.AddModelError("", "Nødvendig status-data mangler i databasen.");
                        return View(hinder); // Send tilbake til skjema med feilmelding
                    }

                    // 3. Sette Tidsstempel (til nå)
                    hinder.Tidsstempel = DateTime.Now;

                    // 4. Sette ApplicationUserId (kobler hinderet til piloten)
                    hinder.ApplicationUserId = user.Id;

                    // 5. Sette StatusID (kobler hinderet til "Ny"-statusen)
                    hinder.StatusID = nyStatus.StatusID;

                    // 6. Lagre det nye hinderet i databasen
                    _context.Add(hinder); // Legger hinderet i "ventesonen"
                    await _context.SaveChangesAsync(); // Lagrer alle endringer fra ventesonen til databasen

                    // 7. Sende brukeren til Hjem-siden
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                // Logg feilen (vi kan legge til logging senere)
                ModelState.AddModelError("", "En feil oppstod under lagring av hinderet.");
            }

            // Hvis vi kommer hit, er ModelState IKKE gyldig (f.eks. "Navn" mangler)
            // eller en feil oppstod. Send brukeren tilbake til skjemaet.
            return View(hinder);
        }
    }
}
