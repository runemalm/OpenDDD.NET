using System.Collections.Generic;
using OpenDDD.Application;

namespace OpenDDD.Domain.Model.BuildingBlocks.Event
{
	public class IntegrationEvent : Event
	{
		public IntegrationEvent() { }
		
		public IntegrationEvent(
			string eventName, 
			DomainModelVersion domainModelVersion, 
			string contextName, 
			ActionId actionId) 
			: base(
				eventName, 
				domainModelVersion, 
				contextName, 
				actionId,
				new List<string>(),
				"",
				"")
		{

		}
	}
}
