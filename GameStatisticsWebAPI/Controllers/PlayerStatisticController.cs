using Common.Models;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;
using RabbitServiceLib.Services;

namespace WebApiService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PlayerStatisticController : ControllerBase
    {
        private readonly RabbitMqConnectionService rabbitService;
        public PlayerStatisticController(RabbitMqConnectionService rabbitMqPublisherService)
        {
            rabbitService = rabbitMqPublisherService;
            if (rabbitService == null) 
            {
                throw new NullReferenceException(nameof(rabbitService));
            }
        }
        [HttpGet("{playerName}")]
        public async Task<ActionResult<PlayerStatistic>> GetPlayerStatistic(string playerName)
        {
            string requestQueueName = "request";
            string responseQueueName = "response";
            var correlationId = Guid.NewGuid().ToString();
            var props = rabbitService.Channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = responseQueueName;

            var requestBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(playerName));
            rabbitService.Channel.BasicPublish(
                exchange: "",
                mandatory: false,
                routingKey: requestQueueName,
                basicProperties: props,
                body: requestBody);

            var consumer = new EventingBasicConsumer(rabbitService.Channel);
            rabbitService.Channel.BasicConsume(queue: responseQueueName, autoAck: false, consumer: consumer);

            PlayerStatistic? result = null;

            consumer.Received += (model, eventArgs) =>
            {
                var responseProps = eventArgs.BasicProperties;
                var responseCorrelationId = responseProps.CorrelationId;
                if (null == correlationId)
                {
                    var responseBytes = eventArgs.Body.ToArray();
                    var responseMessage = Encoding.UTF8.GetString(responseBytes);
                    var response = JsonConvert.DeserializeObject<PlayerStatistic>(responseMessage);
                    result = response;
                }
                rabbitService.Channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            };

            // Wait until the response is received or timeout after 5 seconds
            var timeout = DateTime.Now.AddSeconds(5);
            while (result == null && DateTime.Now < timeout)
            {
                Thread.Sleep(100);
            }

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPatch]
        public async Task<ActionResult<PlayerStatistic>> UpdatePlayersStatistic([FromBody] GameResultDTO stats)
        {
            var queue = "request";

            var props = rabbitService.Channel.CreateBasicProperties();

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(stats));
            rabbitService.Channel.BasicPublish(
                exchange: "",
                mandatory: false,
                routingKey: queue,
                basicProperties: props,
                body: body);

            return Ok();
        }
    }
}
