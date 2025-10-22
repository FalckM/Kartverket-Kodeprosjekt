using Microsoft.EntityFrameworkCore;
using FirstWebApplication.Models;

namespace FirstWebApplication.Models
{
    public class ObstacledbContext : DbContext
    {
        public ObstacledbContext()
        {

        }

        public ObstacledbContext(DbContextOptions<ObstacledbContext> options) : base(options) { }

        public DbSet<ObstacleData> Obstacles { get; set; }

    }
}
