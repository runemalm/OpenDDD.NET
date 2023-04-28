using System;
using OpenDDD.Infrastructure.Ports.PubSub;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.ServiceBus
{
	public class ServiceBusMessage : IPubSubMessage
	{

		public ServiceBusMessage()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
			=> throw new NotImplementedException();
	}
}
