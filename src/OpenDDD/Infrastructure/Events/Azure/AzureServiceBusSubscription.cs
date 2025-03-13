using OpenDDD.Infrastructure.Events.Base;
using Azure.Messaging.ServiceBus;

namespace OpenDDD.Infrastructure.Events.Azure
{
    public class AzureServiceBusSubscription : Subscription<ServiceBusProcessor>
    {
        public AzureServiceBusSubscription(string topic, string consumerGroup, ServiceBusProcessor processor)
            : base(topic, consumerGroup, processor)
        {
            
        }

        public override async ValueTask DisposeAsync()
        {
            if (Consumer.IsProcessing)
            {
                await Consumer.StopProcessingAsync();
            }
            await Consumer.DisposeAsync();
        }
    }
}
