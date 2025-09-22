using System.Diagnostics;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace FirstWebApplication.Controllers
{
    //public class HomeController : Controller
    //{
    //    private readonly ILogger<HomeController> _logger;

    //    //public HomeController(ILogger<HomeController> logger)
    //    //{
    //    //    _logger = logger;
    //    //}


    //    private readonly string _connectionString;

    //    public HomeController(IConfiguration config)
    //    {
    //        _connectionString = config.GetConnectionString("DefaultConnection");
    //    }


    //    //public async Task<IActionResult> Index()
    //    //{
    //    //  try
    //    //{
    //    //  await using var conn = new MySqlConnection(_connectionString);
    //    //await conn.OpenAsync();
    //    //return Content("Connected to MariaDB successfully!");

    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //return Content("Failed to connect to MariaDB " + ex.Message);
    //    //}
    //    //}

    //    [HttpGet]
    //    public ActionResult DataForm()
    //    {
    //        return View();
    //    }

    //    [HttpPost]
    //    public ActionResult DataForm(ObstacleData obstacledata)
    //    {
    //        return View("Overview", obstacledata);
    //    }

    //    public IActionResult Privacy()
    //    {
    //        return View();
    //    }

    //    public IActionResult Index()
    //    {
    //        return View();
    //    }

    //    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //    public IActionResult Error()
    //    {
    //        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    //    }
    //}

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IActionResult> Index()
        {
            string viewModel1 = "Connected to MariaDB successfully!";
            string viewModel2 = "Failed to connect to MariaDB";

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                return View("Index", viewModel1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MariaDB");
                return View("Index", viewModel2 + " " + ex.Message);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
