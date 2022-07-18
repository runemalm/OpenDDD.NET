using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using KellermanSoftware.CompareNetObjects;

namespace DDD.Tests
{
    public class UnitTests : IDisposable
    {
        public UnitTests()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Tests");
            Environment.SetEnvironmentVariable("ENV_FILE", "env.local.test");
            ConfigureJsonConvert();
		}

        public void Dispose()
        {
	        UnsetConfigEnvironmentVariables();
        }
        
        // Configuration
        
        public void UnsetConfigEnvironmentVariables()
        {
	        foreach(DictionaryEntry e in Environment.GetEnvironmentVariables())
	        {
		        if (e.Key.ToString().StartsWith("CFG_"))
			        Environment.SetEnvironmentVariable(e.Key.ToString(), null);
	        }
        }

		private void ConfigureJsonConvert()
		{
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Converters = new List<JsonConverter>()
				{
					new StringEnumConverter
					{
						AllowIntegerValues = false,
						NamingStrategy = new DefaultNamingStrategy()
					}
				},
				DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffK",
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.None
			};
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

		public void AssertResponse(object expected, object actual)
			=> AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(expected, actual);
		
		public void AssertPersisted(object expected, object actual)
			=> AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(expected, actual);
		
		public void AssertDeepEqualIgnoreIdsAndDateTimeDiff1Sec(object expected, object actual)
		{
			CompareLogic compareLogic = new CompareLogic();
			compareLogic.Config.MembersToIgnore = new List<string>() { "*Id" };
			compareLogic.Config.MaxDifferences = 100;
			compareLogic.Config.MaxMillisecondsDateDifference = 999;
			compareLogic.Config.IgnoreCollectionOrder = true;

			ComparisonResult result = compareLogic.Compare(expected, actual);

			Assert.True(result.AreEqual, result.DifferencesString);
		}
	}
}
