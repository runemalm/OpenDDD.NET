using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.ServiceBus
{
	public class ServiceBusSettings : IServiceBusSettings
	{
		public string ConnString { get; }
		public string SubName { get; }

		public ServiceBusSettings(IOptions<Options> options)
		{
			var connString = options.Value.SERVICEBUS_CONN_STRING;
			var subName = options.Value.SERVICEBUS_SUB_NAME;

			ConnString = connString;
			SubName = subName;
		}
	}
}
