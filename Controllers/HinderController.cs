using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NRLWebApp.Data;
using NRLWebApp.Models.Entities;
using System.Threading.Tasks;

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

        [Authorize(Roles = "Registerfører, Admin")]
        public async Task<IActionResult> Index()
        {
            var hindre = await _context.Hindre
                .Include(h => h.Status)
                .Include(h => h.HinderType)
                .Include(h => h.ApplicationUser) 
                    .ThenInclude(u => u.Organisasjon) 
                .ToListAsync();

            return View(hindre);
        }

        [Authorize(Roles = "Pilot")]
        public async Task<IActionResult> MineHindre()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); 
            }

            var mineHindre = await _context.Hindre
                .Where(h => h.ApplicationUserId == user.Id) 
                .Include(h => h.Status)
                .Include(h => h.HinderType)
                .OrderByDescending(h => h.Tidsstempel) 
                .ToListAsync();

            return View(mineHindre);
        }

        [Authorize(Roles = "Pilot")]
        public async Task<IActionResult> UferdigeHindre()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var uferdigeHindre = await _context.Hindre
                .Where(h => h.ApplicationUserId == user.Id && h.Status.Navn == "Uferdig") 
                .Include(h => h.Status)
                .Include(h => h.HinderType)
                .OrderByDescending(h => h.Tidsstempel)
                .ToListAsync();

            return View(uferdigeHindre);
        }

        [Authorize(Roles = "Pilot")]
        public async Task<IActionResult> PilotEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hinder = await _context.Hindre
                .Include(h => h.Status)
                .FirstOrDefaultAsync(m => m.HinderID == id && m.ApplicationUserId == user.Id);

            if (hinder == null)
            {
                return NotFound(); 
            }

            if (hinder.Status.Navn != "Uferdig")
            {
 
                return RedirectToAction(nameof(MineHindre));
            }

            ViewBag.HinderTypeID = new SelectList(_context.HinderTyper, "HinderTypeID", "Navn", hinder.HinderTypeID);
            return View(hinder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pilot")]
        public async Task<IActionResult> PilotEdit(int id, [Bind("HinderID,HinderTypeID,Hoyde,Beskrivelse,Lokasjon")] Hinder hinder)
        {
            if (id != hinder.HinderID)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hinderToUpdate = await _context.Hindre
                .FirstOrDefaultAsync(h => h.HinderID == id && h.ApplicationUserId == user.Id);

            if (hinderToUpdate == null)
            {
                return NotFound();
            }

            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("StatusID");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Status");
            ModelState.Remove("Behandlinger");
            ModelState.Remove("HinderType");

            if (ModelState.IsValid)
            {
                try
                {
                    var nyStatus = await _context.Statuser.FirstOrDefaultAsync(s => s.Navn == "Ny");
                    if (nyStatus == null)
                    {
                        ModelState.AddModelError("", "Nødvendig status-data 'Ny' mangler i databasen.");
                        ViewBag.HinderTypeID = new SelectList(_context.HinderTyper, "HinderTypeID", "Navn", hinder.HinderTypeID);
                        return View(hinder);
                    }

                    hinderToUpdate.HinderTypeID = hinder.HinderTypeID;
                    hinderToUpdate.Hoyde = hinder.Hoyde;
                    hinderToUpdate.Beskrivelse = hinder.Beskrivelse;
                    hinderToUpdate.Lokasjon = hinder.Lokasjon; 

                    hinderToUpdate.StatusID = nyStatus.StatusID;

                    _context.Update(hinderToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Hindre.Any(e => e.HinderID == hinder.HinderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MineHindre)); 
            }

            ViewBag.HinderTypeID = new SelectList(_context.HinderTyper, "HinderTypeID", "Navn", hinder.HinderTypeID);
            return View(hinder);
        }

        [Authorize(Roles = "Registerfører, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); 
            }

            var hinder = await _context.Hindre
                .Include(h => h.Status)
                .Include(h => h.ApplicationUser)
                    .ThenInclude(u => u.Organisasjon)
                .FirstOrDefaultAsync(m => m.HinderID == id);

            if (hinder == null)
            {
                return NotFound(); 
            }

            ViewData["StatusID"] = new SelectList(_context.Statuser, "StatusID", "Navn", hinder.StatusID);

            ViewBag.HinderTypeID = new SelectList(_context.HinderTyper, "HinderTypeID", "Navn", hinder.HinderTypeID);

            return View(hinder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Registerfører, Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("HinderID,StatusID")] Hinder hinder, string kommentar)
        {
            if (id != hinder.HinderID)
            {
                return NotFound();
            }

            ModelState.Remove("HinderType");
            ModelState.Remove("Hoyde");
            ModelState.Remove("Beskrivelse");
            ModelState.Remove("Lokasjon");
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Behandlinger");
            ModelState.Remove("Tidsstempel");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                try
                {
                    var hinderToUpdate = await _context.Hindre.FirstOrDefaultAsync(h => h.HinderID == id);
                    if (hinderToUpdate == null)
                    {
                        return NotFound();
                    }

                    hinderToUpdate.StatusID = hinder.StatusID;

                    var user = await _userManager.GetUserAsync(User); 

                    var nyBehandling = new Behandling
                    {
                        HinderID = hinderToUpdate.HinderID,
                        ApplicationUserId = user.Id, 
                        Tidsstempel = DateTime.Now,
                        Kommentar = kommentar 
                    };

                    _context.Add(nyBehandling); 

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Hindre.Any(e => e.HinderID == hinder.HinderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["StatusID"] = new SelectList(_context.Statuser, "StatusID", "Navn", hinder.StatusID);

            var hinderFull = await _context.Hindre
                .Include(h => h.Status)
                .Include(h => h.ApplicationUser)
                    .ThenInclude(u => u.Organisasjon)
                .FirstOrDefaultAsync(m => m.HinderID == id);

            if (hinderFull == null)
            {
                return NotFound();
            }

            hinderFull.StatusID = hinder.StatusID;

            return View(hinderFull); 
        }

        [Authorize(Roles = "Registerfører, Admin, Pilot")] 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinder = await _context.Hindre
                .Include(h => h.Status)
                .Include(h => h.HinderType)
                .Include(h => h.ApplicationUser) 
                    .ThenInclude(u => u.Organisasjon) 
                .Include(h => h.Behandlinger) 
                    .ThenInclude(b => b.ApplicationUser) 
                .FirstOrDefaultAsync(m => m.HinderID == id);

            if (hinder == null)
            {
                return NotFound();
            }

            return View(hinder);
        }

        [Authorize(Roles = "Pilot")]
        public IActionResult Create()
        {
            ViewBag.HinderTypeID = new SelectList(_context.HinderTyper, "HinderTypeID", "Navn");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        [Authorize(Roles = "Pilot")]
        public async Task<IActionResult> Create([Bind("HinderTypeID,Hoyde,Beskrivelse,Lokasjon")] Hinder hinder)
        {
            try
            {

                ModelState.Remove("ApplicationUserId");
                ModelState.Remove("StatusID");
                ModelState.Remove("ApplicationUser");
                ModelState.Remove("Status");
                ModelState.Remove("Behandlinger");
                ModelState.Remove("HinderType");

                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return Challenge();
                    }

                    var nyStatus = await _context.Statuser.FirstOrDefaultAsync(s => s.Navn == "Ny");
                    if (nyStatus == null)
                    {
                        ModelState.AddModelError("", "Nødvendig status-data mangler i databasen.");
                        return View(hinder); 
                    }

                    hinder.Tidsstempel = DateTime.Now;

                    hinder.ApplicationUserId = user.Id;

                    hinder.StatusID = nyStatus.StatusID;

                    _context.Add(hinder); 
                    await _context.SaveChangesAsync(); 

                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "DEBUG-FEIL: " + ex.Message);

                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", "INDRE FEIL: " + ex.InnerException.Message);
                }
            }

            ViewBag.HinderTypeID = new SelectList(_context.HinderTyper, "HinderTypeID", "Navn", hinder.HinderTypeID);

            return View(hinder);
        }

        [HttpPost]
        [Authorize(Roles = "Pilot")]
        public async Task<IActionResult> HurtigRegistrer(string lokasjon)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var uferdigStatus = await _context.Statuser.FirstOrDefaultAsync(s => s.Navn == "Uferdig");
            if (uferdigStatus == null)
            {
                return StatusCode(500, "Databasen mangler 'Uferdig' status.");
            }

            var hinder = new Hinder
            {
                ApplicationUserId = user.Id,
                Lokasjon = lokasjon,
                Tidsstempel = DateTime.Now,
                StatusID = uferdigStatus.StatusID,

                HinderTypeID = null,
                Hoyde = null,
                Beskrivelse = "" 
            };

            try
            {
                _context.Add(hinder);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Hinder hurtigregistrert!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "En feil oppstod ved lagring: " + ex.Message);
            }
        }
    }
}
