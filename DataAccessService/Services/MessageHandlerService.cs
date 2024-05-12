using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitServiceLib.Services;

namespace DataAccessService.Services
{
    internal class MessageHandlerService : IHostedService
    {
        private readonly DatabaseAccessService dbAccess;
        private readonly RabbitMqConnectionService rabbitService;
        public MessageHandlerService(DatabaseAccessService dbAccessService, RabbitMqConnectionService rabbitMqService)
        {
            dbAccess = dbAccessService;
            rabbitService = rabbitMqService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Clean up any resources here
            return Task.CompletedTask;
        }
    }
}
