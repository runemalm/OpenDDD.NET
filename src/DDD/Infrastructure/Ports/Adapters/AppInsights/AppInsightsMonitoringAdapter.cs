using System;
using Microsoft.ApplicationInsights;

namespace DDD.Infrastructure.Ports.Adapters.AppInsights
{
	public class AppInsightsMonitoringAdapter : IMonitoringPort
	{
		private readonly TelemetryClient _telemetryClient;

		public AppInsightsMonitoringAdapter(TelemetryClient telemetryClient)
		{
			_telemetryClient = telemetryClient;
		}

		public T TrackDependency<T>(Func<T> method, string className, string methodName, string data)
		{
			var success = true;
			var startTime = DateTime.UtcNow;
			var timer = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				return method();
			}
			catch (Exception e)
			{
				success = false;
				throw e;
			}
			finally
			{
				timer.Stop();
				_telemetryClient.TrackDependency(
					className,
					methodName,
					data,
					startTime,
					timer.Elapsed,
					success);
			}
		}
	}
}
