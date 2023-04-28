using System.Collections.Generic;
using OpenDDD.Application;

namespace OpenDDD.Domain.Model.BuildingBlocks.Event
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
