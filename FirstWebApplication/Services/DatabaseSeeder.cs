using FirstWebApplication.Data;
using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Services
{
    /// <summary>
    /// Service for å generere realistisk testdata til den normaliserte databasen.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Random _random;

        public DatabaseSeeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

            // 2. Seed obstacle types
            var obstacleTypes = await SeedObstacleTypesAsync();
            Console.WriteLine($"✅ {obstacleTypes.Count} obstacle types opprettet");

            // 3. Seed obstacles (hindringer)
            await SeedObstaclesAsync(pilots, obstacleTypes, 150);
            Console.WriteLine("✅ 150 hindringer opprettet");

            Console.WriteLine("🎉 Database seeding fullført!");
        }

        /// <summary>
        /// Oppretter obstacle types hvis de ikke finnes
        /// </summary>
        private async Task<List<ObstacleType>> SeedObstacleTypesAsync()
        {
            var existingTypes = await _context.ObstacleTypes.ToListAsync();
            if (existingTypes.Any())
            {
                return existingTypes;
            }

            var types = new List<ObstacleType>
            {
                new ObstacleType { Name = "Mast", Description = "Radio/TV mast", MinHeight = 10, MaxHeight = 500 },
                new ObstacleType { Name = "Tower", Description = "Tower structure", MinHeight = 20, MaxHeight = 300 },
                new ObstacleType { Name = "Power Line", Description = "Power line/cables", MinHeight = 5, MaxHeight = 100 },
                new ObstacleType { Name = "Wind Turbine", Description = "Wind turbine", MinHeight = 50, MaxHeight = 250 },
                new ObstacleType { Name = "Building", Description = "Building", MinHeight = 10, MaxHeight = 200 },
                new ObstacleType { Name = "Crane", Description = "Construction crane", MinHeight = 20, MaxHeight = 150 },
                new ObstacleType { Name = "Bridge", Description = "Bridge", MinHeight = 10, MaxHeight = 100 },
                new ObstacleType { Name = "Other", Description = "Other obstacles", MinHeight = 1, MaxHeight = 1000 }
            };

            _context.ObstacleTypes.AddRange(types);
            await _context.SaveChangesAsync();

            return types;
        }

        /// <summary>
        /// Oppretter piloter med unike e-postadresser og navn.
        /// </summary>
        private async Task<List<ApplicationUser>> SeedPilotsAsync(int count)
        {
            var pilots = new List<ApplicationUser>();

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

                var user = new ApplicationUser
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
        /// Oppretter obstacles med forskjellige statuser.
        /// </summary>
        private async Task<List<Obstacle>> SeedObstaclesAsync(List<ApplicationUser> pilots, List<ObstacleType> obstacleTypes, int count)
        {
            var obstacles = new List<Obstacle>();

            var descriptions = new[]
            {
                "Høy mast med røde og hvite striper",
                "Gammel tårnstruktur med rustne deler",
                "Ny moderne konstruksjon",
                "Midlertidig struktur - under bygging",
                "Permanent installasjon med lys",
                "Høyspentledning - farlig område",
                "Industriell konstruksjon",
                "Kommunikasjonsutstyr montert på toppen"
            };

            // Norwegian coordinates (around Oslo, Bergen, Trondheim, Stavanger)
            var locations = new[]
            {
                (59.9139, 10.7522),  // Oslo
                (60.3913, 5.3221),   // Bergen
                (63.4305, 10.3951),  // Trondheim
                (58.9700, 5.7331),   // Stavanger
                (69.6492, 18.9553),  // Tromsø
                (62.4722, 6.1549),   // Ålesund
                (61.1153, 10.4662),  // Lillehammer
                (58.1467, 7.9956)    // Kristiansand
            };

            for (int i = 0; i < count; i++)
            {
                var pilot = pilots[_random.Next(pilots.Count)];
                var obstacleType = obstacleTypes[_random.Next(obstacleTypes.Count)];
                var location = locations[_random.Next(locations.Length)];

                // Add some random offset to location
                var lat = location.Item1 + (_random.NextDouble() - 0.5) * 0.5;
                var lng = location.Item2 + (_random.NextDouble() - 0.5) * 0.5;

                var obstacle = new Obstacle
                {
                    Name = $"{obstacleType.Name} #{i + 1}",
                    Height = (decimal)(_random.Next(10, 200) + _random.NextDouble()),
                    Description = descriptions[_random.Next(descriptions.Length)],
                    Location = $"POINT({lng} {lat})",
                    ObstacleTypeId = obstacleType.Id,
                    RegisteredByUserId = pilot.Id,
                    RegisteredDate = DateTime.Now.AddDays(-_random.Next(1, 90))
                };

                _context.Obstacles.Add(obstacle);
                await _context.SaveChangesAsync();

                // Determine status based on random
                int statusRoll = _random.Next(100);
                int statusTypeId;

                if (statusRoll < 60)  // 60% Pending
                {
                    statusTypeId = 2; // Pending
                }
                else if (statusRoll < 85)  // 25% Approved
                {
                    statusTypeId = 3; // Approved
                }
                else  // 15% Rejected
                {
                    statusTypeId = 4; // Rejected
                }

                // Create initial status
                var status = new ObstacleStatus
                {
                    ObstacleId = obstacle.Id,
                    StatusTypeId = statusTypeId,
                    ChangedByUserId = pilot.Id,
                    ChangedDate = obstacle.RegisteredDate,
                    Comments = statusTypeId == 2 ? "Awaiting review" :
                               statusTypeId == 3 ? "Looks good" :
                               "Incomplete information",
                    IsActive = true
                };

                _context.ObstacleStatuses.Add(status);
                await _context.SaveChangesAsync();

                // Update obstacle's CurrentStatusId
                obstacle.CurrentStatusId = status.Id;
                _context.Obstacles.Update(obstacle);
                await _context.SaveChangesAsync();

                obstacles.Add(obstacle);
            }

            return obstacles;
        }
    }
}