namespace DDD.Domain.Model.BuildingBlocks.Event
{
	public interface IEvent : IBuildingBlock
	{
		EventHeader Header { get; set; }

		void AddDeliveryFailure(string error);
	}
}
