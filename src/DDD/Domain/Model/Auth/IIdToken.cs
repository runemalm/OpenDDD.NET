using System;
using System.Collections.Generic;
using System.Security.Claims;
using DDD.Domain.Model.BuildingBlocks.ValueObject;

namespace DDD.Domain.Model.Auth
{
	public interface IIdToken : IValueObject
	{
		string Actor { get; set; }
		IEnumerable<string> Audiences { get; set; }
		IEnumerable<Claim> Claims { get; set; }
		string Issuer { get; set; }
		DateTime ValidFrom { get; set; }
		DateTime ValidTo { get; set; }

		IEnumerable<string> GetClaimsValues(IEnumerable<string> types);
		string GetClaimValue(string type);
		IEnumerable<string> GetClaimListValue(string type);
	}
}
