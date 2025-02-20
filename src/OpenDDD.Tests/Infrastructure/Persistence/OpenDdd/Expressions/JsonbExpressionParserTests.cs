using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Expressions;
using OpenDDD.Tests.Base;

namespace OpenDDD.Tests.Infrastructure.Persistence.OpenDdd.Expressions
{
    public class JsonbExpressionParserTests : UnitTests
    {
        private class Customer
        {
            public int Age { get; set; }
            public string Name { get; set; } = string.Empty;
            public Address Address { get; set; } = new();
            public string? MiddleName { get; set; }
            public bool IsActive { get; set; }
        }

        private class Address
        {
            public string City { get; set; } = string.Empty;
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForNumericComparison()
        {
            Expression<Func<Customer, bool>> expression = c => c.Age > 30;
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("(data->>'age')::numeric > 30");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForStringEquality()
        {
            Expression<Func<Customer, bool>> expression = c => c.Name == "John Doe";
            var result = JsonbExpressionParser.Parse(expression);
    
            result.Should().Be("data->>'name' = 'John Doe'");
        }
        
        [Fact]
        public void Parse_ShouldGenerateSql_ForStringEquality_WithVariable()
        {
            var name = "John Doe";
            Expression<Func<Customer, bool>> expression = c => c.Name == name;
            var result = JsonbExpressionParser.Parse(expression);
    
            result.Should().Be("data->>'name' = 'John Doe'");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForStringContains()
        {
            Expression<Func<Customer, bool>> expression = c => c.Name.Contains("John");
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("data->>'name' ILIKE '%' || 'John' || '%'");
        }
        
        [Fact]
        public void Parse_ShouldGenerateSql_ForStringContains_WithCapturedVariableMemberExpression()
        {
            var command = new { Name = "John" };
            Expression<Func<Customer, bool>> expression = c => c.Name.Contains(command.Name);

            var result = JsonbExpressionParser.Parse(expression);
    
            result.Should().Be("data->>'name' ILIKE '%' || 'John' || '%'");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForStringStartsWith()
        {
            Expression<Func<Customer, bool>> expression = c => c.Name.StartsWith("John");
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("data->>'name' ILIKE 'John' || '%'");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForStringEndsWith()
        {
            Expression<Func<Customer, bool>> expression = c => c.Name.EndsWith("Doe");
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("data->>'name' ILIKE '%' || 'Doe'");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForNestedProperty()
        {
            Expression<Func<Customer, bool>> expression = c => c.Address.City == "Paris";
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("data->'address'->>'city' = 'Paris'");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForNullablePropertyCheck()
        {
            Expression<Func<Customer, bool>> expression = c => c.MiddleName != null;
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("data ? 'middlename'");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForBooleanProperty()
        {
            Expression<Func<Customer, bool>> expression = c => c.IsActive;
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("(data->>'isactive')::boolean = TRUE");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForAndCondition()
        {
            Expression<Func<Customer, bool>> expression = c => c.Age > 30 && c.Name.Contains("John");
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("(data->>'age')::numeric > 30 AND data->>'name' ILIKE '%' || 'John' || '%'");
        }

        [Fact]
        public void Parse_ShouldGenerateSql_ForOrCondition()
        {
            Expression<Func<Customer, bool>> expression = c => c.Age > 30 || c.Address.City == "Paris";
            var result = JsonbExpressionParser.Parse(expression);
            
            result.Should().Be("(data->>'age')::numeric > 30 OR data->'address'->>'city' = 'Paris'");
        }
    }
}
