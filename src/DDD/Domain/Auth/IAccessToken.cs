using DDD.Application.Settings;
using Domain.Model.AuthFlow;

namespace DDD.Domain.Auth
{
	public interface IAccessToken : IValueObject
	{
		AuthMethod AuthMethod { get; set; }
		TokenType TokenType { get; set; }
		string RawString { get; set; }
	}
}
