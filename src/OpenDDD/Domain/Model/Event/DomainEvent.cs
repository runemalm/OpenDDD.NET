using System.Collections.Generic;
using OpenDDD.Application;
using OpenDDD.NET;

namespace OpenDDD.Domain.Model.Event
{
	public class DomainEvent : Event, IDomainEvent 
	{
		public DomainEvent() { }
			
		public DomainEvent(
			string eventName, 
			BaseDomainModelVersion domainModelVersion, 
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
