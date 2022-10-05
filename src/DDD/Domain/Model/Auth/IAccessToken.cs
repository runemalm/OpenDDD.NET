using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.ValueObject;

namespace DDD.Domain.Model.Auth
{
	public interface IAccessToken : IValueObject
	{
		AuthMethod AuthMethod { get; set; }
		TokenType TokenType { get; set; }
		string RawString { get; set; }
	}
}
