namespace DDD.Domain.Model.Error
{
	public interface IDomainError
	{
		int Code { get; set; }
		string Message { get; set; }
		string UserMessage { get; set; }
	}
}
