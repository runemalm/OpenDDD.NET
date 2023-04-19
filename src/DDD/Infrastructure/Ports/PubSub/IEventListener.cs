using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain.Model;

namespace DDD.Infrastructure.Ports.PubSub
{
	public interface IEventListener
	{
		public Context Context { get; }
		public string ListensTo { get; }
		public DomainModelVersion ListensToVersion { get; }

		void Start();
		Task StartAsync();
		void Stop();
		Task StopAsync();
		Task<bool> React(IPubSubMessage message);
		Task Handle(IPubSubMessage message);
	}
}
