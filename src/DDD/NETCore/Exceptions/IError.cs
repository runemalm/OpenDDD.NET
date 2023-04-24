namespace DDD.NETCore.Exceptions
{
	public interface IError
	{
		int Code { get; set; }
		string Message { get; set; }
		string UserMessage { get; set; }
	}
}
