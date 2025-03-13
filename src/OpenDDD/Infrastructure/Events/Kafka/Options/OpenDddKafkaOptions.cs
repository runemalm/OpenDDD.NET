﻿namespace OpenDDD.Infrastructure.Events.Kafka.Options
{
    public class OpenDddKafkaOptions
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public bool AutoCreateTopics { get; set; } = true;
    }
}
