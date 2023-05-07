using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Ports.PubSub
{
	public interface IEventListener
	{
		public Context Context { get; }
		public string ListensTo { get; }
		public string ActionName { get; }
		public DomainModelVersion ListensToVersion { get; }

		void Start();
		Task StartAsync();
		void Stop();
		Task StopAsync();
		Task<bool> React(IPubSubMessage message);
		Task<bool> Handle(IPubSubMessage message);
	}
}
