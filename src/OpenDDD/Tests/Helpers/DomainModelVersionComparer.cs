﻿using System;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using OpenDDD.Domain.Model;

namespace OpenDDD.Tests.Helpers
{
	public class DomainModelVersionComparer : BaseTypeComparer
	{
		/*
		 * A custom comparer for CompareNetObjects.
		 *
		 * Compares two DomainModelVersion objects on the _version property.
		 * Disregards if one of the objects are a subclass of base class.
		 */
		public DomainModelVersionComparer(RootComparer rootComparer) : base(rootComparer)
		{
		}

		public override bool IsTypeMatch(Type type1, Type type2)
		{
			if (type1 != null ^ type2 != null)
				return false;
			var theType = typeof(BaseDomainModelVersion);
			var isMatch =
				(type1 == theType || type1.IsSubclassOf(theType)) &&
				(type2 == theType || type2.IsSubclassOf(theType));
			return isMatch;
		}

		public override void CompareType(CompareParms parms)
		{
			BaseDomainModelVersion casted1 = (BaseDomainModelVersion)parms.Object1;
			BaseDomainModelVersion casted2 = (BaseDomainModelVersion)parms.Object2;

			if (!casted1.Equals(casted2))
			{
				Difference difference = new Difference
				{
					PropertyName = parms.BreadCrumb,
					Object1Value = casted1.ToString(),
					Object2Value = casted2.ToString()
				};

				parms.Result.Differences.Add(difference);
			}
		}
	}
}
