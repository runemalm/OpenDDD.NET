using OpenDDD.NET;

namespace OpenDDD.Domain.Model.BuildingBlocks.Event
{
	public interface IEvent : IBuildingBlock
	{
		EventHeader Header { get; set; }

		void AddDeliveryFailure(string error, IDateTimeProvider dateTimeProvider);
	}
}
