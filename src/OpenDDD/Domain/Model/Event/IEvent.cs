using OpenDDD.NET;

namespace OpenDDD.Domain.Model.Event
{
	public interface IEvent
	{
		EventHeader Header { get; set; }

		void AddDeliveryFailure(string error, IDateTimeProvider dateTimeProvider);
	}
}
