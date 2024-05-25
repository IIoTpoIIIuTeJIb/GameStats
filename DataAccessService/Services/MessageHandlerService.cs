using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using RabbitServiceLib.Services;
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
            consumer.Received += OnMessageReceived;
            while (true)
            {
                _rabbitService.Channel.BasicConsume(queue: requestQueueName, autoAck: false, consumer: consumer);
            }
        }

        private void OnMessageReceived(object? model, BasicDeliverEventArgs e)
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
                    var responseBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    var propsResponse = _rabbitService.Channel.CreateBasicProperties();
                    propsResponse.CorrelationId = props.CorrelationId;
                    _rabbitService.Channel.BasicPublish(exchange: "", routingKey: responseQueueName, basicProperties: propsResponse, body: responseBody);
                    break;
                case "PATCH":
                    if (wrapper.Data is GameResultDTO)
                    {
                        _dataService.Patch((GameResultDTO)wrapper.Data);
                    }
                    break;
                default:
                    break;
            }

            _rabbitService.Channel.BasicAck(e.DeliveryTag, false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
