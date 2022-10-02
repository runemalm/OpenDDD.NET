using System.Collections.Generic;

namespace DDD.Domain
{
	public class DomainEvent : Event 
	{
		public DomainEvent() { }
		
		public DomainEvent(
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
