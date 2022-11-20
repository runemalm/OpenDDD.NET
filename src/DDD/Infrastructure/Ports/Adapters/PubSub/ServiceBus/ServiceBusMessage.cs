using System;
using DDD.Infrastructure.Ports.PubSub;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.ServiceBus
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
