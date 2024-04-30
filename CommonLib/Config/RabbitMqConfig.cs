namespace GameStatisticsWebAPI.Config
{
    public class RabbitMqConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string RoutingKey { get; set; }
        public int RetryCount { get; set; }
        public int ReconnectInterval { get; set; }
    }
}
