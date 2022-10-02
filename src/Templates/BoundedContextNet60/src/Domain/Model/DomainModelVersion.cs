namespace Domain.Model
{
	public class DomainModelVersion : DDD.Domain.DomainModelVersion
	{
		public DomainModelVersion(string dotString) : base(dotString) { }
		public DomainModelVersion(int major, int minor, int build) : base(major, minor, build) { }

		public static DomainModelVersion Latest()
		{
			return new DomainModelVersion("1.0.0");
		}
	}
}
