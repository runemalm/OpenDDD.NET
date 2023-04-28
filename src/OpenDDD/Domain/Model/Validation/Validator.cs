using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

namespace OpenDDD.Domain.Model.Validation
{
	public class Validator<TModel>
	{
		private const string StringNumberPattern = @"^(-?[0-9][0-9]*)$";
		private const string EmailPattern = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])";
		private const string PhonePattern = @"^[\d ()+-]{8,}$";

		protected readonly TModel _objectToValidate;
		private readonly List<ValidationError> _errors = new List<ValidationError>();

		public Validator(TModel objectToValidate)
		{
			_objectToValidate = objectToValidate;
		}

		public IEnumerable<ValidationError> Errors() => _errors;

		public Validator<TModel> NotNull()
		{
			NotNullViolated(_objectToValidate, typeof(TModel).Name);

			return this;
		}

		public Validator<TModel> NotNull<TProperty>(Expression<Func<TModel, TProperty>> field)
		{
			NotNullViolated(GetValue(field), GetKey(field));
			return this;
		}

		public Validator<TModel> NotEqual<TProperty>(Expression<Func<TModel, TProperty>> field,
			TProperty referenceValue)
			where TProperty : IEquatable<TProperty>
		{
			NotEqualViolated(GetKey(field), GetValue(field), referenceValue);
			return this;
		}

		public Validator<TModel> NotEqual(Expression<Func<TModel, Enum>> field,
			Enum referenceValue)
		{
			NotEqualViolated(GetKey(field), GetValue(field), referenceValue);
			return this;
		}

		public Validator<TModel> NotEmpty(Expression<Func<TModel, Guid>> field)
		{
			NotEmptyViolated(GetValue(field), GetKey(field));
			return this;
		}

		public Validator<TModel> NotNullOrEmpty(Expression<Func<TModel, string>> field)
		{
			NotEmptyViolated(GetValue(field), GetKey(field));
			return this;
		}

		public Validator<TModel> NotNullOrEmpty(Expression<Func<TModel, Guid?>> field)
		{
			var key = GetKey(field);
			var value = GetValue(field);

			if (!NotNullViolated(value, key))
				NotEmptyViolated(value.Value, key);

			return this;
		}

		public Validator<TModel> NotNullOrEmpty<TProperty>(Expression<Func<TModel, IEnumerable<TProperty>>> field)
		{
			var key = GetKey(field);
			var value = GetValue(field);

			if (!NotNullViolated(value, key))
				NotEmptyViolated(value, key);

			return this;
		}
		
		public Validator<TModel> NotBothNullOrEmpty<TProperty>(Expression<Func<TModel, IEnumerable<TProperty>>> field1, Expression<Func<TModel, IEnumerable<TProperty>>> field2)
		{
			var key1 = GetKey(field1);
			var value1 = GetValue(field1);
			
			var key2 = GetKey(field2);
			var value2 = GetValue(field2);

			if (value1.IsNullOrEmpty() && value2.IsNullOrEmpty())
				AddError(key1+", "+key2, "Both values can't be null or empty.");
			
			return this;
		}

		public Validator<TModel> NotNullAndPositive(Expression<Func<TModel, decimal?>> field)
		{
			var key = GetKey(field);
			var value = GetValue(field);

			if (!NotNullViolated(value, key))
				NotNegativeViolated(value.Value, key);

			return this;
		}

		public Validator<TModel> NotNegative(Expression<Func<TModel, decimal>> field)
		{
			NotNegativeViolated(GetValue(field), GetKey(field));
			return this;
		}

		public Validator<TModel> NotZero(Expression<Func<TModel, decimal>> field)
		{
			NotZeroViolated(GetValue(field), GetKey(field));
			return this;
		}

		public Validator<TModel> StringNumber(Expression<Func<TModel, string>> field)
		{
			var key = GetKey(field);
			var value = GetValue(field);

			if (!NotNullViolated(value, key))
				StringNumberViolated(value, key);

			return this;
		}

		public Validator<TModel> Email(Expression<Func<TModel, string>> field)
		{
			var key = GetKey(field);
			var value = GetValue(field);

			if (!NotNullViolated(value, key))
				EmailViolated(value, key);

			return this;
		}

		public Validator<TModel> Phone(Expression<Func<TModel, string>> field)
		{
			var key = GetKey(field);
			var value = GetValue(field);

			if (!NotNullViolated(value, key))
				PhoneViolated(value, key);

			return this;
		}

		public Validator<TModel> Func(Func<IEnumerable<ValidationError>> func)
		{
			if (func != null) _errors.AddRange(func());
			return this;
		}

		protected bool NotNullViolated<TProperty>(TProperty value, string key)
			=> ConstraintViolated(value == null, key, "cannot be null.");

		protected bool NotEqualViolated<TProperty>(string key, TProperty actualValue, TProperty referenceValue)
			where TProperty : IEquatable<TProperty>
			=> ConstraintViolated(referenceValue.Equals(actualValue), key, $"cannot be {referenceValue}.");

		protected bool NotEqualViolated(string key, Enum actualValue, Enum referenceValue)
			=> ConstraintViolated(referenceValue.ToString() == actualValue.ToString(), key, $"cannot be {referenceValue}.");
		
		protected bool StringNumberViolated(string value, string key)
			=> ConstraintViolated(!Regex.IsMatch(value, StringNumberPattern, RegexOptions.IgnoreCase), key, $"\"{value}\" is not a valid string number");

		protected bool EmailViolated(string value, string key)
			=> ConstraintViolated(!Regex.IsMatch(value, EmailPattern, RegexOptions.IgnoreCase), key, $"\"{value}\" is not a valid email");

		protected bool PhoneViolated(string value, string key)
			=> ConstraintViolated(!Regex.IsMatch(value, PhonePattern), key, $"\"{value}\" is not a valid phone number");

		protected bool NotNegativeViolated(decimal value, string key)
			=> ConstraintViolated(value < 0, key, $"\"{value}\" is less than zero");

		protected bool NotZeroViolated(decimal value, string key)
			=> ConstraintViolated(value == 0, key, "is zero");

		protected bool NotEmptyViolated(Guid value, string key)
			=> ConstraintViolated(value == default, key, "cannot be empty");

		protected bool NotEmptyViolated(string value, string key)
			=> ConstraintViolated(string.IsNullOrWhiteSpace(value), key, "cannot be null or empty");

		protected bool NotEmptyViolated<TProperty>(IEnumerable<TProperty> value, string key)
			=> ConstraintViolated(!(value?.Any() ?? false), key, "cannot be null or empty");

		protected string GetKey<TP>(Expression<Func<TModel, TP>> field)
		{
			var propName = "";
			switch (field.Body)
			{
				case MemberExpression memberExpression:
					propName = memberExpression.Member.Name;
					break;
				case UnaryExpression unaryExpression:
					propName = ((MemberExpression)unaryExpression.Operand).Member.Name;
					break;
			}

			return $"{typeof(TModel).Name}.{propName}";
		}

		protected TP GetValue<TP>(Expression<Func<TModel, TP>> field)
		{
			try
			{
				return field.Compile()(_objectToValidate);
			}
			catch (NullReferenceException)
			{
				return default(TP);
			}
		}

		protected void AddError(string key, string details)
		{
			_errors.Add(
				new ValidationError
				{
					Key = key,
					Details = details
				}
			);
		}

		private bool ConstraintViolated(bool violated, string key, string violation)
		{
			if (violated)
				AddError(key, violation);
			return violated;
		}
	}
}