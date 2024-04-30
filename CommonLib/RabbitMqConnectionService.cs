using GameStatisticsWebAPI.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Channels;

namespace GameStatisticsWebAPI
{
    public class RabbitMqConnectionService : IHostedService, IDisposable
    {
        private RabbitMqConfig rabbitConfig;
        public IConnection? Connection { get; set; }
        public IModel? Channel { get; set; }

        public RabbitMqConnectionService(IOptions<RabbitMqConfig> rabbitConfig)
        {
            this.rabbitConfig = rabbitConfig.Value;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ConnectionFactory factory = CreateConnectionFactory();
            Connection = CreateConnection(factory);
            if (Connection is null)
            {
                throw new Exception("Невозможно создать соединение с брокером сообщений");
            }
            Channel = Connection.CreateModel();
            if (Connection is null)
            {
                throw new Exception("Невозможно создать канал с брокером сообщений");
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = rabbitConfig.RetryCount != 0 ? true : false,
                HostName = "localhost",
                //Port = rabbitConfig.Port,
                UserName = rabbitConfig.Username,
                Password = rabbitConfig.Password
            };
            if (rabbitConfig.RetryCount != 0)
            {
                factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(rabbitConfig.ReconnectInterval);
            }
            if (rabbitConfig.RetryCount < 0)
            {
                factory.RequestedConnectionTimeout = TimeSpan.FromDays(2);
            }
            else if (rabbitConfig.RetryCount > 0)
            {
                factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(rabbitConfig.ReconnectInterval * rabbitConfig.RetryCount);
            }
            return factory;
        }

        private IConnection? CreateConnection(ConnectionFactory factory)
        {
            if (factory is not null)
            {
                IConnection rabbitConnection = factory.CreateConnection();
                return rabbitConnection;
            }
            else
            {
                return null;
            }
        }

        private IModel? CreateChannel(IConnection connection)
        {
            if (connection == null)
            {
                return null;
            }
            Channel = connection.CreateModel();
            return Channel;
        }

        private EventingBasicConsumer CreateConsumer(IModel channel, string queueName, Action<object, BasicDeliverEventArgs> action)
        {
            if (action is null)
            {
                throw new Exception("Требуется передать действие на событие нового сообщения от RabbitMQ");
            }
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += new EventHandler<BasicDeliverEventArgs>(action);
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            return consumer;
        }

        public void Dispose()
        {
            if (Channel is not null)
            {
                Channel.Close();
                Channel.Dispose();
            }
            if (Connection is not null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }
    }
}
