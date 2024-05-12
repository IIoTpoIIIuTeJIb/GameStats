using DataAccessService.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DataAccessService.Services
{
    internal class DatabaseAccessService : IHostedService, IDisposable
    {
        //private PostgresQlConfig config;
        private readonly DbContextOptions<ApplicationContext> _dbContextOptions;

        public DatabaseAccessService()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            optionsBuilder.UseNpgsql("Your PostgreSQL connection string");
            _dbContextOptions = optionsBuilder.Options;
        }

        public void Dispose()
        {
            // TODO: Dispose database connection
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //var connectionString = config.GetConnectionString("DefaultConnection");
            //var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            //var options = optionsBuilder.UseNpgsql(connectionString).Options;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}