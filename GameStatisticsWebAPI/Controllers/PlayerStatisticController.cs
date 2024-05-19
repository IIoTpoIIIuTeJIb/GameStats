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
        private readonly RabbitMqConnectionService _rabbitService;
        string requestQueueName = "request";
        string responseQueueName = "response";
        public PlayerStatisticController(RabbitMqConnectionService rabbitMqPublisherService)
        {
            _rabbitService = rabbitMqPublisherService;
            if (_rabbitService == null) 
            {
                throw new NullReferenceException(nameof(_rabbitService));
            }
        }

        [HttpGet("{playerName}")]
        public ActionResult<PlayerStatsDTO> GetPlayerStatistic(string playerName)
        {
            if (_rabbitService is null)
            {
                throw new NullReferenceException("Rabbit Service is null");
            }
            if (_rabbitService.Channel is null)
            {
                throw new NullReferenceException("Rabbit Channel is null");
            }
            var correlationId = Guid.NewGuid().ToString();
            var props = _rabbitService.Channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = responseQueueName;
            var wrapper = new RabbitWrapper();
            wrapper.Message = "GET";
            wrapper.Data = playerName;

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wrapper));

            _rabbitService.Channel.BasicPublish(
                exchange: "",
                mandatory: false,
                routingKey: requestQueueName,
                basicProperties: props,
                body: body
                );

            var consumer = new EventingBasicConsumer(_rabbitService.Channel);

            PlayerStatsDTO? result = null;
            EventHandler<BasicDeliverEventArgs> handler = (model, eventArgs) =>
            {
                var responseProps = eventArgs.BasicProperties;
                var responseCorrelationId = responseProps.CorrelationId;
                if (true)//responseCorrelationId == correlationId)
                {
                    var responseBytes = eventArgs.Body.ToArray();
                    var responseMessage = Encoding.UTF8.GetString(responseBytes);
                    var response = JsonConvert.DeserializeObject<PlayerStatsDTO>(responseMessage);
                    if (response == null)
                    {
                        throw new NullReferenceException("Response is null");
                    }
                    result = response;
                    Console.WriteLine(result);
                }
                Console.WriteLine("corr ID" + correlationId);
                Console.WriteLine("response corr ID" + responseCorrelationId);
            };
            consumer.Received += handler;
            _rabbitService.Channel.BasicConsume(queue: responseQueueName, autoAck: true, consumer: consumer);


            var timeout = DateTime.Now.AddSeconds(3);
            while (result == null && DateTime.Now < timeout)
            {
                Thread.Sleep(100);
            }
            consumer.Received -= handler;

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpPatch]
        public async Task<ActionResult<GameResultDTO>> UpdatePlayersStatistic([FromBody] GameResultDTO stats)
        {
            if (_rabbitService is null)
            {
                throw new NullReferenceException("Rabbit Service is null");
            }
            if (_rabbitService.Channel is null)
            {
                throw new NullReferenceException("Rabbit Channel is null");
            }

            var props = _rabbitService.Channel.CreateBasicProperties();
            var wrapper = new RabbitWrapper();
            wrapper.Message = "PATCH";
            wrapper.Data = stats;
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wrapper));
            _rabbitService.Channel.BasicPublish(
                exchange: "",
                mandatory: false,
                routingKey: requestQueueName,
                basicProperties: props,
                body: body);

            return Ok();
        }
    }
}
