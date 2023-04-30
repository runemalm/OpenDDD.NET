namespace Domain.Model
{
	public class DomainModelVersion : OpenDDD.Domain.Model.DomainModelVersion
	{
		public const string LatestString = "1.0.0";

		public DomainModelVersion(string dotString) : base(dotString) { }

		public static DomainModelVersion Latest()
		{
			return new DomainModelVersion(LatestString);
		}
	}
}
