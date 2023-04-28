using System.Text;
using OpenDDD.Infrastructure.Ports.PubSub;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Rabbit
{
	public class RabbitMessage : IPubSubMessage
	{
		public IModel Channel;
		public BasicDeliverEventArgs EventArgs;

		public RabbitMessage(IModel channel, BasicDeliverEventArgs eventArgs)
		{
			Channel = channel;
			EventArgs = eventArgs;
		}

		public override string ToString()
			=> Encoding.UTF8.GetString(EventArgs.Body.ToArray());
	}
}
