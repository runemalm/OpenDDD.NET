using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OpenDDD.NET.Extensions
{
	public static class EnumExtensions
	{
		public static string GetDescriptionFromEnumValue(Enum value)
		{
			DescriptionAttribute attribute = value.GetType()
				.GetField(value.ToString())
				.GetCustomAttributes(typeof (DescriptionAttribute), false)
				.SingleOrDefault() as DescriptionAttribute;
			return attribute == null ? value.ToString() : attribute.Description;
		}

		public static T GetEnumValueFromDescription<T>(string description)
		{
			var type = typeof(T);
			if (!type.IsEnum)
				throw new ArgumentException();
			FieldInfo[] fields = type.GetFields();
			var field = fields
				.SelectMany(f => f.GetCustomAttributes(
					typeof(DescriptionAttribute), false), (
					f, a) => new { Field = f, Att = a })
				.Where(a => ((DescriptionAttribute)a.Att)
					.Description == description).SingleOrDefault();
			return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
		}
	}
}
