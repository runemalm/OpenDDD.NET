using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;

namespace DDD.Infrastructure.Ports
{
	public interface IEventListener
	{
		public abstract Context Context { get; }
		public abstract string ListensTo { get; }
		public abstract IDomainModelVersion ListensToVersion { get; }

		Task Start();
		Task<bool> React(IPubSubMessage message);
	}
}
