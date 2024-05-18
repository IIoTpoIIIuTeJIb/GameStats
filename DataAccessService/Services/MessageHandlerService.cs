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
using System.Diagnostics.Tracing;
using Common.Models;
using Newtonsoft.Json;
using Common.DTO;

namespace DataAccessService.Services
{
    internal class MessageHandlerService : IHostedService
    {
        private readonly ApplicationContext _context;
        private readonly RabbitMqConnectionService _rabbitService;
        private readonly DatabaseAccessService _dataService;

        private string requestQueueName = "request";
        private string responseQueueName = "response";
        public MessageHandlerService(DatabaseAccessService databaseAccessService, RabbitMqConnectionService rabbitMqService)
        {
            _dataService = databaseAccessService;
            _rabbitService = rabbitMqService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_rabbitService is null)
            {
                throw new NullReferenceException("Rabbit Service is null");
            }
            if (_rabbitService.Channel is null)
            {
                throw new NullReferenceException("Rabbit Channel is null");
            }

            ConsumeMessage();
            return Task.CompletedTask;
        }

        private void ConsumeMessage()
        {

            var consumer = new EventingBasicConsumer(_rabbitService.Channel);
            consumer.Received += onMessageReceived;
            while (true)
            {
                _rabbitService.Channel.BasicConsume(queue: requestQueueName, autoAck: false, consumer: consumer);
            }
        }

        private void onMessageReceived(object? model, BasicDeliverEventArgs e)
        {
            var props = e.BasicProperties;
            //TODO: process request
            var body = e.Body.ToArray();
            var jsonBody = Encoding.UTF8.GetString(body);
            var wrapper = JsonConvert.DeserializeObject<RabbitWrapper>(jsonBody);

            if (wrapper is null)
            {
                return;
            }
            switch (wrapper.Message)
            {
                case "GET":
                    var result = _dataService.Get((string)wrapper.Data);
                    var responseWrapper = new RabbitWrapper
                    {
                        Message = "Response",
                        Data = result
                    };
                    var responseBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseWrapper));
                    var propsResponse = _rabbitService.Channel.CreateBasicProperties();
                    _rabbitService.Channel.BasicPublish(exchange: "", routingKey: responseQueueName, basicProperties: propsResponse, body: responseBody);
                    break;
                case "PATCH":
                    _dataService.Patch((GameResultDTO)wrapper.Data);
                    break;
                default:
                    break;
            }

            _rabbitService.Channel.BasicAck(e.DeliveryTag, false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Clean up any resources here
            return Task.CompletedTask;
        }
    }
}
