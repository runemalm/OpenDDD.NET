using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Event;
using OpenDDD.NET.Services.Outbox;

namespace OpenDDD.Infrastructure.Ports.Events
{
	public class DomainPublisher : BasePublisher<IDomainEvent>, IDomainPublisher
	{
		public DomainPublisher(ILogger<DomainPublisher> logger, IActionOutbox outbox) : base(logger, outbox)
		{
			
		}
	}
}
