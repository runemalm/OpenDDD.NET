using OpenDDD.Infrastructure.Services.Publisher;

namespace OpenDDD.Domain.Model.Event
{
	public interface IDomainPublisher : IPublisher<IDomainEvent>
	{
		
	}
}
