using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace GameStatisticsWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PlayerStatisticController : ControllerBase
    {
        private readonly RabbitMqConnectionService RabbitService;
        public PlayerStatisticController(RabbitMqConnectionService rabbitMqPublisherService)
        {
            RabbitService = rabbitMqPublisherService;
        }
        [HttpGet("{playerName}")]
        public async Task<ActionResult<PlayerStatistic>> GetPlayerStatistic(string playerName)
        {

            var result = new PlayerStatistic(); //TODO: RabbitMQ
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPatch]
        public async Task<ActionResult<PlayerStatistic>> UpdatePlayersStatistic([FromBody] PlayerStatistic stats)
        {
            stats = new();
            var queue = "request";
            var props = RabbitService.Channel.CreateBasicProperties();
            
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(stats));
            //TODO: RabbitMQ
            try
            {
                RabbitService.Channel.BasicPublish(
                    exchange: "",
                    mandatory: false,
                    routingKey: queue,
                    basicProperties: props,
                    body: body);
            }
            catch (Exception ex)
            {
                throw new Exception("Невозможно подключиться к Rabbit");
            }

            return Ok("message");
        }
    }
}
