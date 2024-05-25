using RabbitServiceLib.Config;
using System.Text.Json.Serialization;
using RabbitServiceLib.Services;

namespace WebApiService
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
