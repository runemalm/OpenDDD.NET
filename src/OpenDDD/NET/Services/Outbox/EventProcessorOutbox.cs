using Microsoft.Extensions.Logging;
using OpenDDD.NET.Services.DatabaseConnection;

namespace OpenDDD.NET.Services.Outbox
{
	public class EventProcessorOutbox : Infrastructure.Services.Outbox.Outbox, IEventProcessorOutbox
	{
		public EventProcessorOutbox(IEventProcessorDatabaseConnection databaseConnection, ILogger<EventProcessorOutbox> logger) 
			: base(databaseConnection, logger)
		{
			
		}
	}
}
