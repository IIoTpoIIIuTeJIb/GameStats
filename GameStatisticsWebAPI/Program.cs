
using GameStatisticsWebAPI.Config;
using System.Net;
using System.Text.Json.Serialization;

namespace GameStatisticsWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigurationManager configuration = builder.Configuration;

            builder.Services.Configure<RabbitMqConfig>(configuration.GetSection("RabbitMQConf"));

            builder.Services.AddSingleton<RabbitMqConnectionService>();
            builder.Services.AddSingleton<IHostedService, RabbitMqConnectionService>(serviceProvider => serviceProvider.GetService<RabbitMqConnectionService>());
            //builder.Services.AddHostedService<RabbitMqConnectionService>();
            //services.AddSingleton<ActorSystemBackgroundService>();
            //services.AddSingleton<IHostedService, ActorSystemBackgroundService>(
            //    serviceProvider => serviceProvider.GetService<ActorSystemBackgroundService>());

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddControllers(options =>
            {
                options.AllowEmptyInputInBodyModelBinding = true;
            });

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}
