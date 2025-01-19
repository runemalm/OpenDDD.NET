namespace OpenDDD.Domain.Model
{
	public interface IEntity<TId>
	{
		TId Id { get; }
		DateTime CreatedAt { get; set; }
		DateTime UpdatedAt { get; set; }
	}
}
