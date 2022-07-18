namespace DDD.Domain
{
	public class ValueObject : BuildingBlock, IValueObject
	{
		public ValueObject() {}
		public ValueObject(DomainModelVersion domainModelVersion) : base(domainModelVersion) {}
	}
}
