using System;

namespace OpenDDD.Infrastructure.Ports.Monitoring
{
	public interface IMonitoringPort
	{
		T TrackDependency<T>(Func<T> method, string className, string methodName, string data);
	}
}
