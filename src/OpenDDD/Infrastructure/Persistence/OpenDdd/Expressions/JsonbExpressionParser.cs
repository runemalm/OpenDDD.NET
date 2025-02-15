using System.Linq.Expressions;
using System.Reflection;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Expressions
{
    public static class JsonbExpressionParser
    {
        public static string Parse<T>(Expression<Func<T, bool>> expression)
        {
            return ParseExpression(expression.Body);
        }

        private static string ParseExpression(Expression expression)
        {
            switch (expression)
            {
                case BinaryExpression binary:
                    return ParseBinaryExpression(binary);

                case MemberExpression member:
                    return ParseMemberExpression(member);

                case ConstantExpression constant:
                    return ParseConstantExpression(constant);

                case MethodCallExpression methodCall:
                    return ParseMethodCallExpression(methodCall);

                case UnaryExpression unary when unary.NodeType == ExpressionType.Convert:
                    return ParseExpression(unary.Operand); // Strip unnecessary type conversions

                default:
                    if (IsCapturedVariable(expression))
                        return ParseConstantExpression(EvaluateExpression(expression)); // Extract value from closure variable

                    throw new NotSupportedException($"Unsupported expression type: {expression.NodeType}");
            }
        }
        
        private static string ParseBinaryExpression(BinaryExpression expression)
        {
            var left = ParseExpression(expression.Left);
            var right = ParseExpression(expression.Right);
            var op = GetSqlOperator(expression.NodeType);
        
            // Ensure captured variables are evaluated properly
            if (IsCapturedVariable(expression.Right))
                right = ParseExpression(EvaluateExpression(expression.Right));
            
            // Special case for JSONB NULL checks
            if (expression.Right is ConstantExpression constant && constant.Value == null)
            {
                if (expression.NodeType == ExpressionType.NotEqual)
                {
                    return $"{ExtractJsonbPath(left)} ? '{ExtractJsonbKey(left)}'";
                }
                else if (expression.NodeType == ExpressionType.Equal)
                {
                    return $"NOT ({ExtractJsonbPath(left)} ? '{ExtractJsonbKey(left)}')";
                }
            }
        
            return $"{left} {op} {right}";
        }
        
        private static string ExtractJsonbKey(string jsonbPath)
        {
            var parts = jsonbPath.Split("->>");
            return parts.Last().Trim('\'');
        }

        private static string ExtractJsonbPath(string jsonbPath)
        {
            var parts = jsonbPath.Split("->>");
            return string.Join("->", parts.Take(parts.Length - 1));
        }

        private static string ParseMemberExpression(MemberExpression expression)
        {
            var propertyPath = GetJsonbPath(expression);
            var memberType = GetMemberType(expression);

            // Convert JSONB fields to appropriate PostgreSQL types
            if (memberType == typeof(bool))
                return $"({propertyPath})::boolean = TRUE"; // Cast JSONB boolean fields
            if (IsNumericType(memberType))
                return $"({propertyPath})::numeric"; // Cast JSONB numeric fields

            return propertyPath;
        }

        private static string ParseConstantExpression(ConstantExpression expression)
        {
            return expression.Value switch
            {
                null => "NULL",
                bool boolVal => boolVal ? "TRUE" : "FALSE",
                int or long or double or float or decimal => expression.Value.ToString()!,
                _ => $"'{expression.Value}'" // Wrap string values in single quotes
            };
        }

        private static string ParseMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Object is not MemberExpression member)
                throw new NotSupportedException($"Unsupported method call on non-member: {expression.Method.Name}");

            var instance = ParseExpression(expression.Object!);
            var argument = ParseExpression(expression.Arguments[0]);

            return expression.Method.Name switch
            {
                "Contains" => $"{instance} ILIKE '%' || {argument} || '%'",
                "StartsWith" => $"{instance} ILIKE {argument} || '%'",
                "EndsWith" => $"{instance} ILIKE '%' || {argument}",
                _ => throw new NotSupportedException($"Unsupported method: {expression.Method.Name}")
            };
        }
        
        private static bool IsCapturedVariable(Expression expression)
        {
            return expression is MemberExpression { Expression: ConstantExpression };
        }

        private static ConstantExpression EvaluateExpression(Expression expression)
        {
            if (expression is MemberExpression member && member.Expression is ConstantExpression constant)
            {
                var value = ((FieldInfo)member.Member).GetValue(constant.Value);
                return Expression.Constant(value);
            }

            throw new InvalidOperationException("Failed to evaluate expression.");
        }

        private static string GetSqlOperator(ExpressionType nodeType) => nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Unsupported operator: {nodeType}")
        };

        private static string GetJsonbPath(MemberExpression expression)
        {
            var properties = new List<string>();
            Expression? current = expression;

            while (current is MemberExpression member)
            {
                properties.Insert(0, member.Member.Name.ToLower()); // Insert at start to preserve order
                current = member.Expression;
            }

            if (properties.Count == 1)
            {
                return $"data->>'{properties[0]}'";
            }

            var path = string.Join("->", properties.Take(properties.Count - 1).Select(p => $"'{p}'"));
            var lastProperty = properties.Last();

            return $"data->{path}->>'{lastProperty}'";
        }

        private static Type GetMemberType(MemberExpression member)
        {
            return member.Member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new InvalidOperationException("Unknown member type")
            };
        }

        private static bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(double) ||
                   type == typeof(float) || type == typeof(decimal);
        }
    }
}
