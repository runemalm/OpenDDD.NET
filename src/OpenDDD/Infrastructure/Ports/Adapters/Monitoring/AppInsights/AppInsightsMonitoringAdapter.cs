using System;
using Microsoft.ApplicationInsights;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.NET;

namespace OpenDDD.Infrastructure.Ports.Adapters.Monitoring.AppInsights
{
	public class AppInsightsMonitoringAdapter : IMonitoringPort
	{
		private readonly TelemetryClient _telemetryClient;
		private readonly IDateTimeProvider _dateTimeProvider;

		public AppInsightsMonitoringAdapter(TelemetryClient telemetryClient, IDateTimeProvider dateTimeProvider)
		{
			_telemetryClient = telemetryClient;
			_dateTimeProvider = dateTimeProvider;
		}

		public T TrackDependency<T>(Func<T> method, string className, string methodName, string data)
		{
			var success = true;
			var startTime = _dateTimeProvider.Now;
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
