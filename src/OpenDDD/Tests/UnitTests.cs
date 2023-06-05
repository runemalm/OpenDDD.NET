using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using OpenDDD.NET;
using OpenDDD.Tests.Helpers;

namespace OpenDDD.Tests
{
    public class UnitTests
    {
        public UnitTests()
        {
	        Environment.SetEnvironmentVariable("_TestExplorer_TestResultMessageMaxLength_", "9000");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Tests");
        }

        // Assertions

		public void AssertObjectsEqual(object obj1, object obj2)
		{
			CompareLogic compareLogic = new CompareLogic();
			compareLogic.Config.MaxDifferences = 100;
			compareLogic.Config.MaxMillisecondsDateDifference = 999;
			compareLogic.Config.IgnoreCollectionOrder = true;
			compareLogic.Config.IgnoreObjectTypes = true;
			
			ComparisonResult result = compareLogic.Compare(obj1, obj2);

			Assert.True(result.AreEqual, result.DifferencesString);
		}

		// Helpers

		public string RandomString(int length)
		{
			var str = "";

			while (str.Length < length)
				str += Guid.NewGuid().ToString("n");

			return str.Substring(0, length);
		}

		public string GetEnumMemberAttrValue<T>(T enumVal)
		{
			/*
             * Returns the "Value" attribute of an EnumMember.
             */
			var enumType = typeof(T);
			var memInfo = enumType.GetMember(enumVal.ToString());
			var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
			if (attr != null)
			{
				return attr.Value;
			}

			return null;
		}

		public Task<string> GetRandomIdentityAsync()
		{
			return Task.FromResult(Guid.NewGuid().ToString().ToUpper());
		}
		
		// Assertion
		
		protected void AssertTrue(bool condition)
			=> Assert.True(condition);

		protected void AssertTrue(bool condition, string userMessage)
			=> Assert.True(condition, userMessage);
        
		protected void AssertTrue(bool? condition, string userMessage)
			=> Assert.True(condition, userMessage);
		
		protected void AssertCount(int expected, int actual)
			=> AssertEqual(expected, actual);
		
		protected void AssertGreater(int greaterThan, int actual)
			=> Assert.True(actual > greaterThan, $"Expected actual count to be greater than {greaterThan}.");
		
		protected void AssertCount<T>(int expected, IEnumerable<T> collection)
			=> AssertCount(expected, collection.Count());
		
		protected void AssertContains<T>(T expected, IEnumerable<T> collection)
			=> Assert.Contains(expected, collection);

		protected void AssertContains<T>(IEnumerable<T> expected, IEnumerable<T> collection)
		{
			var missing = new List<T>();
			foreach (var e in expected)
				if (!collection.Contains(e))
					missing.Add(e);
			if (missing.Count > 0)
			{
				Assert.True(false, $"The collection doesn't contain {missing.Count} missing item(s): {String.Join(", ", missing.Select(m => m.ToString()))}");
			}
		}

		protected void AssertNotNull(object? @object) 
			=> Assert.NotNull(@object);
		
		protected void AssertNull(object? @object) 
			=> Assert.Null(@object);

		protected void AssertNotEqual<T>(T expected, T actual) 
			=> Assert.NotEqual(expected, actual);

		protected void AssertEqual<T>(T expected, T actual) 
			=> Assert.Equal(expected, actual);
		
		protected void AssertEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual) 
			=> Assert.Equal<T>(expected, actual);

		protected void AssertEqual<T>(
			IEnumerable<T> expected,
			IEnumerable<T> actual,
			IEqualityComparer<T> comparer)
		{
			Assert.Equal(expected, actual, comparer);
		}

		protected void AssertNow(DateTime? actual)
			=> AssertDateWithin400Ms(DateTimeProvider.Now, actual, "The date wasn't equal or close to 'now'.");
		
		protected void AssertDateApproximately(DateTime expected, DateTime actual)
			=> AssertDateWithin200Ms(expected, actual, "The date wasn't approximately what was expected.");

		protected void AssertDateWithinMs(DateTime expected, DateTime? actual, double ms, string? message)
		{
			if (actual == null && expected != null)
				throw new Exception("TODO: Find out how to throw assert exception like the built in methods here...");
			Assert.True((expected - actual) < TimeSpan.FromMilliseconds(ms), message ?? $"The date wasn't within {ms}ms of expected date.");
		}
		
		protected void AssertDateEqualOrCloseTo(DateTime expected, DateTime? actual)
			=> AssertDateWithin200Ms(expected, actual, "The date wasn't equal or close to expected date.");

		protected void AssertDateWithin200Ms(DateTime expected, DateTime? actual, string? message = null)
			=> AssertDateWithinMs(expected, actual, 200, message ?? "The date wasn't within 200ms of expected date.");
		
		protected void AssertDateWithin400Ms(DateTime expected, DateTime? actual, string? message = null)
			=> AssertDateWithinMs(expected, actual, 400, message ?? "The date wasn't within 400ms of expected date.");

		public void AssertEntity(object expected, object actual)
			=> AssertEntities(new List<object>{expected}, new List<object>{actual});

		public void AssertEntities(object expected, object actual)
			=> AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(expected, actual);

		public void AssertPersisted(object expected, object actual)
			=> AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(expected, actual);
		
		public void AssertEvent(object expected, object actual)
			=> AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(expected, actual);
		
		public void AssertResponse(object expected, object actual)
			=> AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(expected, actual);
		
		public void AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(object expected, object actual)
		{
			CompareLogic compareLogic = new CompareLogic();
			compareLogic.Config.MembersToIgnore = new List<string>() { "*Id" };
			compareLogic.Config.MaxDifferences = 100;
			compareLogic.Config.MaxMillisecondsDateDifference = 999;
			compareLogic.Config.IgnoreCollectionOrder = true;
			compareLogic.Config.CustomComparers = 
				new List<BaseTypeComparer>
				{
					new DomainModelVersionComparer(RootComparerFactory.GetRootComparer())
				};

			ComparisonResult result = compareLogic.Compare(expected, actual);

			Assert.True(result.AreEqual, result.DifferencesString);
		}
    }
}
