using Microsoft.Extensions.Options;

namespace OpenDDD.Application.Settings.Monitoring
{
	public class MonitoringSettings : IMonitoringSettings
	{
		public MonitoringProvider Provider { get; set; }

		public MonitoringSettings() { }

		public MonitoringSettings(IOptions<Options> options)
		{
			var provider = MonitoringProvider.None;
			var providerString = options.Value.MONITORING_PROVIDER;
			if (providerString != null)
				if (providerString.ToLower() == "memory")
					provider = MonitoringProvider.Memory;
				else if (providerString.ToLower() == "appinsights")
					provider = MonitoringProvider.AppInsights;

			Provider = provider;
		}
	}
}
