using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessService
{
    internal class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerStatistic> PlayerStats { get; set; }
    }
}
