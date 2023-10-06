using System.Threading.Tasks;
using OpenDDD.Infrastructure.Ports.MessageBroker;

namespace OpenDDD.Infrastructure.Services.EventPublisher
{
	public interface IMessagePublisher
	{
		Task PublishAsync(IMessage message, Topic topic);
	}
}
