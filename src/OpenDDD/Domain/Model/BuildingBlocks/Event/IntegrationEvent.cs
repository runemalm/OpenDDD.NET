using System.Collections.Generic;
using OpenDDD.Application;
using OpenDDD.NET;

namespace OpenDDD.Domain.Model.BuildingBlocks.Event
{
	public class IntegrationEvent : Event
	{
		public IntegrationEvent() { }
		
		public IntegrationEvent(
			string eventName, 
			DomainModelVersion domainModelVersion, 
			string contextName,
			IDateTimeProvider dateTimeProvider,
			ActionId actionId) 
			: base(
				eventName, 
				domainModelVersion, 
				contextName, 
				actionId,
				new List<string>(),
				"",
				"",
				dateTimeProvider)
		{

		}
	}
}
