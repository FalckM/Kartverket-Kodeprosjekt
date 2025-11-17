using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Services
{
    /// <summary>
    /// Forbedret service for å generere realistisk testdata til databasen.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Random _random;

        public DatabaseSeeder(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _random = new Random();
        }

        /// <summary>
        /// Hovedmetode for å seede alle data.
        /// </summary>
        public async Task SeedAllDataAsync()
        {
            Console.WriteLine("🌱 Starter database seeding...");

            // 1. Seed brukere (piloter)
            var pilots = await SeedPilotsAsync(20);
            Console.WriteLine($"✅ {pilots.Count} piloter opprettet");

            // 2. Seed obstacles (hindringer)
            await SeedObstaclesAsync(pilots, 150);
            Console.WriteLine("✅ 150 hindringer opprettet");

            Console.WriteLine("🎉 Database seeding fullført!");
        }

        /// <summary>
        /// Oppretter piloter med unike e-postadresser og navn.
        /// </summary>
        private async Task<List<IdentityUser>> SeedPilotsAsync(int count)
        {
            var pilots = new List<IdentityUser>();

            // Norske fornavn og etternavn for realistisk data
            var firstNames = new[] {
                "Ole", "Kari", "Per", "Lise", "Lars", "Anne", "Erik", "Marit",
                "Jon", "Ingrid", "Bjørn", "Silje", "Tom", "Hanne", "Anders",
                "Nina", "Kristian", "Emma", "Martin", "Sara", "Henrik", "Sofie",
                "Magnus", "Thea", "Andreas", "Julie", "Daniel", "Maria"
            };

            var lastNames = new[] {
                "Hansen", "Johansen", "Olsen", "Larsen", "Andersen", "Pedersen",
                "Nilsen", "Kristiansen", "Jensen", "Karlsen", "Johnsen", "Pettersen",
                "Eriksen", "Berg", "Haugen", "Hagen", "Bakken", "Strand", "Lie", "Bye",
                "Moen", "Lund", "Solberg", "Holm", "Dahl"
            };

            for (int i = 0; i < count; i++)
            {
                string firstName = firstNames[_random.Next(firstNames.Length)];
                string lastName = lastNames[_random.Next(lastNames.Length)];
                string email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@pilot.no";

                // Sjekk om bruker allerede eksisterer
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    pilots.Add(existingUser);
                    continue;
                }

                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Pilot123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Pilot");
                    pilots.Add(user);
                }
            }

            return pilots;
        }

        /// <summary>
        /// Oppretter hindringer med realistiske norske lokasjoner, koordinater og varierte tidspunkt.
        /// </summary>
        private async Task<int> SeedObstaclesAsync(List<IdentityUser> pilots, int count)
        {
            // Norske byer med koordinater (latitude, longitude)
            var norwegianLocations = new[]
            {
                new { City = "Oslo", Lat = 59.9139, Lon = 10.7522 },
                new { City = "Bergen", Lat = 60.3913, Lon = 5.3221 },
                new { City = "Trondheim", Lat = 63.4305, Lon = 10.3951 },
                new { City = "Stavanger", Lat = 58.9700, Lon = 5.7331 },
                new { City = "Tromsø", Lat = 69.6492, Lon = 18.9553 },
                new { City = "Kristiansand", Lat = 58.1599, Lon = 7.9956 },
                new { City = "Drammen", Lat = 59.7439, Lon = 10.2045 },
                new { City = "Fredrikstad", Lat = 59.2181, Lon = 10.9298 },
                new { City = "Porsgrunn", Lat = 59.1403, Lon = 9.6561 },
                new { City = "Ålesund", Lat = 62.4722, Lon = 6.1549 },
                new { City = "Bodø", Lat = 67.2804, Lon = 14.4049 },
                new { City = "Haugesund", Lat = 59.4138, Lon = 5.2680 },
                new { City = "Tønsberg", Lat = 59.2676, Lon = 10.4073 },
                new { City = "Moss", Lat = 59.4349, Lon = 10.6570 },
                new { City = "Sandefjord", Lat = 59.1312, Lon = 10.2166 }
            };

            // Hindertyper med realistiske høyder
            var obstacleTypes = new[]
            {
                new { Type = "Radio Tower", MinHeight = 30, MaxHeight = 80 },
                new { Type = "TV Mast", MinHeight = 40, MaxHeight = 90 },
                new { Type = "Mobile Antenna", MinHeight = 15, MaxHeight = 45 },
                new { Type = "Power Line", MinHeight = 20, MaxHeight = 50 },
                new { Type = "Wind Turbine", MinHeight = 60, MaxHeight = 100 },
                new { Type = "Building", MinHeight = 15, MaxHeight = 85 },
                new { Type = "Church Spire", MinHeight = 25, MaxHeight = 70 },
                new { Type = "Crane", MinHeight = 30, MaxHeight = 75 },
                new { Type = "Smoke Stack", MinHeight = 35, MaxHeight = 80 },
                new { Type = "Monument", MinHeight = 10, MaxHeight = 40 }
            };

            var obstacles = new List<ObstacleData>();

            for (int i = 0; i < count; i++)
            {
                // Velg tilfeldig lokasjon
                var location = norwegianLocations[_random.Next(norwegianLocations.Length)];

                // Legg til realistisk variasjon i koordinater (+/- 0.3 grader = ca. 30km)
                double lat = location.Lat + (_random.NextDouble() - 0.5) * 0.6;
                double lon = location.Lon + (_random.NextDouble() - 0.5) * 0.6;

                // Velg tilfeldig hindring type
                var obstacleType = obstacleTypes[_random.Next(obstacleTypes.Length)];

                // Generer realistisk høyde basert på type
                double height = _random.Next(obstacleType.MinHeight, obstacleType.MaxHeight + 1);

                // Velg tilfeldig pilot
                var pilot = pilots[_random.Next(pilots.Count)];

                // Generer tilfeldig dato OG tidspunkt
                var registeredDate = GenerateRandomDateTime();

                // Bestem status (70% approved, 20% pending, 10% rejected)
                int statusRoll = _random.Next(100);
                bool isApproved = statusRoll < 70;
                bool isRejected = !isApproved && statusRoll < 90;

                var obstacle = new ObstacleData
                {
                    // UTEN bynavn - bare type og nummer
                    ObstacleName = $"{obstacleType.Type} #{i + 1}",
                    ObstacleHeight = height,
                    ObstacleDescription = $"{obstacleType.Type} near {location.City}. Height: {height}m. Registered for aviation safety.",

                    // MED koordinater hvis feltene finnes
                    // Uncomment hvis du har disse feltene:
                    // Latitude = lat,
                    // Longitude = lon,
                    // GeometryType = "Point",
                    // Coordinates = $"POINT({lon} {lat})",

                    RegisteredBy = pilot.Email,
                    RegisteredDate = registeredDate,
                    IsApproved = isApproved,
                    IsRejected = isRejected
                };

                obstacles.Add(obstacle);
            }

            // Lagre til databasen
            await _context.Obstacles.AddRangeAsync(obstacles);
            await _context.SaveChangesAsync();

            return obstacles.Count;
        }

        /// <summary>
        /// Genererer tilfeldig dato MED variert tidspunkt mellom 2023-2025.
        /// </summary>
        private DateTime GenerateRandomDateTime()
        {
            // 20% sjanse for 2023, 30% for 2024, 50% for 2025
            int yearRoll = _random.Next(100);
            int year;

            if (yearRoll < 20)
                year = 2023;
            else if (yearRoll < 50)
                year = 2024;
            else
                year = 2025;

            // Tilfeldig måned (1-12)
            int month = _random.Next(1, 13);

            // Tilfeldig dag (1-28 for å unngå problemer med februar)
            int day = _random.Next(1, 29);

            // NYTT: Tilfeldig tidspunkt på dagen (0-23 timer, 0-59 minutter)
            int hour = _random.Next(0, 24);
            int minute = _random.Next(0, 60);
            int second = _random.Next(0, 60);

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}