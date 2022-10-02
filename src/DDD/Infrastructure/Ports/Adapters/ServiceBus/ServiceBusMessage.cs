using System;

namespace DDD.Infrastructure.Ports.Adapters.ServiceBus
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
