using System;

namespace DDD.Infrastructure.Ports.Adapters.Memory
{
	public class MemoryMonitoringAdapter : IMonitoringPort
	{
		public MemoryMonitoringAdapter()
		{
			
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
				// TODO: Add stats to some in-memory variable..
			}
		}
	}
}
