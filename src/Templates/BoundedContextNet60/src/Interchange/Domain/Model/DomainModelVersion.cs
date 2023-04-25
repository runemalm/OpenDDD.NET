﻿namespace Interchange.Domain.Model
{
	public class DomainModelVersion : DDD.Domain.Model.DomainModelVersion
	{
		public const string LatestString = "1.0.0";

		public DomainModelVersion(string dotString) : base(dotString) { }

		public static DomainModelVersion Latest()
		{
			return new DomainModelVersion(LatestString);
		}
	}
}
