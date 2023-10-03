using OpenDDD.Domain.Model;

namespace MyBoundedContext.Domain.Model
{
	public class DomainModelVersion : BaseDomainModelVersion
	{
		public const string LatestString = "1.0.0";
		
		public DomainModelVersion(string dotString) : base(dotString) { }

		public static DomainModelVersion Latest()
		{
			return new DomainModelVersion(LatestString);
		}
	}
}
