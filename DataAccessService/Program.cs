using DataAccessService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitServiceLib.Config;
using RabbitServiceLib.Services;
using DataAccessService.Config;
using Serilog;

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
                services.Configure<SQLiteConfig>(configuration.GetSection("SQLiteConnectionStrings"));
                services.AddDbContext<ApplicationContext>(options =>
                    options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ) );
                services.AddSingleton<RabbitMqConnectionService>();
                services.AddSingleton<IHostedService, RabbitMqConnectionService>(serviceProvider => serviceProvider.GetService<RabbitMqConnectionService>());
                services.AddHostedService<DatabaseAccessService>();
                services.AddSingleton<DatabaseAccessService>();
                services.AddHostedService<MessageHandlerService>();
                services.AddSingleton<MessageHandlerService>();
            }
        );
        }
    }
}
