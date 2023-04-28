using OpenDDD.Domain.Model.BuildingBlocks.ValueObject;

namespace OpenDDD.Domain.Model.Auth
{
	public interface IAccessToken : IValueObject
	{
		AuthMethod AuthMethod { get; set; }
		TokenType TokenType { get; set; }
		string RawString { get; set; }
	}
}
