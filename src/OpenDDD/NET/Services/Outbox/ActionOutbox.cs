using Microsoft.Extensions.Logging;
using OpenDDD.NET.Services.DatabaseConnection;

namespace OpenDDD.NET.Services.Outbox
{
	public class ActionOutbox : Infrastructure.Services.Outbox.Outbox, IActionOutbox
	{
		public ActionOutbox(IActionDatabaseConnection databaseConnection, ILogger<ActionOutbox> logger) 
			: base(databaseConnection, logger)
		{
			
		}
	}
}
