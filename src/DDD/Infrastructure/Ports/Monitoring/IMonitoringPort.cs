using System;

namespace DDD.Infrastructure.Ports.Monitoring
{
	public interface IMonitoringPort
	{
		T TrackDependency<T>(Func<T> method, string className, string methodName, string data);
	}
}
