namespace OpenDDD.Domain.Model
{
	public interface IAggregateRoot<TId> : IEntity<TId>
	{
		DateTime CreatedAt { get; set; }
		DateTime UpdatedAt { get; set; }
	}
}
