using System;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.NET;

namespace OpenDDD.Infrastructure.Ports.Adapters.Monitoring.Memory
{
	public class MemoryMonitoringAdapter : IMonitoringPort
	{
		private readonly IDateTimeProvider _dateTimeProvider;
		
		public MemoryMonitoringAdapter(IDateTimeProvider dateTimeProvider)
		{
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
				// TODO: Add stats to some in-memory variable..
			}
		}
	}
}
