namespace OpenDDD.Domain.Model
{
	public interface IEntity<TId>
	{
		TId Id { get; }
	}
}
