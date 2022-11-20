using System.Collections.Generic;
using DDD.Application;

namespace DDD.Domain.Model.BuildingBlocks.Event
{
	public class DomainEvent : Event 
	{
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
