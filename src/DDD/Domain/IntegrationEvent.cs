using System.Collections.Generic;

namespace DDD.Domain
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
