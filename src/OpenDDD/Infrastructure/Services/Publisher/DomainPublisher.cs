using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model.Event;
using OpenDDD.NET.Services.Outbox;

namespace OpenDDD.Infrastructure.Services.Publisher
{
	public class DomainPublisher : BasePublisher<IDomainEvent>, IDomainPublisher
	{
		public DomainPublisher(ILogger<DomainPublisher> logger, IActionOutbox outbox) : base(logger, outbox)
		{
			
		}
	}
}
