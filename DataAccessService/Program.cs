using DataAccessService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitServiceLib.Config;
using RabbitServiceLib.Services;
using DataAccessService.Config;

namespace DataAccessService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                services.Configure<RabbitMqConfig>(configuration.GetSection("RabbitMQConf"));
                services.Configure<PostgresQlConfig>(configuration.GetSection("ConnectionStrings"));
                services.AddHostedService<DatabaseAccessService>();
                services.AddHostedService<RabbitMqConnectionService>();
                services.AddDbContext<ApplicationContext>();
            }
        );
        }
    }
}
