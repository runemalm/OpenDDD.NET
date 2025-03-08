namespace OpenDDD.Infrastructure.Events.RabbitMq.Options
{
    public class OpenDddRabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public bool AutoCreateTopics { get; set; } = true;
    }
}
