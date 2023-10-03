using System.Collections.Generic;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.Database;
using OpenDDD.Tests.Helpers;

namespace OpenDDD.Infrastructure.Services.Outbox
{
	public class Outbox : IOutbox, IInfrastructureService
	{
		private readonly ILogger _logger;
		private readonly IDatabaseConnection _databaseConnection;
		private const string DatabaseCollectionName = "outbox";
		
		public Outbox(IDatabaseConnection databaseConnection, ILogger<Outbox> logger)
		{
			_databaseConnection = databaseConnection;
			_logger = logger;
		}
		
		public async Task AddEventAsync(IEvent theEvent)
		{
			await _databaseConnection.AddDocumentAsync(DatabaseCollectionName, theEvent);
			_logger.LogDebug("Adding event to outbox.");
		}

		public bool HasPublished(IEvent theEvent)
		{
			CompareLogic compareLogic = new CompareLogic();
			compareLogic.Config.MaxDifferences = 1;
			compareLogic.Config.MaxMillisecondsDateDifference = 999;
			compareLogic.Config.IgnoreCollectionOrder = true;
			compareLogic.Config.MembersToIgnore.Add("EventId");
			compareLogic.Config.MembersToIgnore.Add("ActionId");
			compareLogic.Config.CustomComparers = 
				new List<BaseTypeComparer>
				{
					new DomainModelVersionComparer(RootComparerFactory.GetRootComparer())
				};
			
			foreach (var e in _databaseConnection.GetAll<IEvent>(DatabaseCollectionName))
			{
				var result = compareLogic.Compare(theEvent, e);
				if (result.AreEqual)
					return true;
			}
			
			return false;
		}
		
		public Task<bool> HasPublishedAsync(IEvent theEvent)
		{
			return Task.FromResult(HasPublished(theEvent));
		}
	}
}
