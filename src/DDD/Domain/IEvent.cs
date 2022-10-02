namespace DDD.Domain
{
	public interface IEvent : IBuildingBlock
	{
		EventHeader Header { get; set; }

		void AddDeliveryFailure(string error);
	}
}
