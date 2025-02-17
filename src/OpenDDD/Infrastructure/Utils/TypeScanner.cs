using System.Reflection;

namespace OpenDDD.Infrastructure.Utils
{
    public static class TypeScanner
    {
        private static readonly object _assemblyCacheLock = new();
        private static IEnumerable<Assembly>? _cachedAssemblies;

        public static IEnumerable<Assembly> GetRelevantAssemblies(bool includeDynamic = false)
        {
            if (_cachedAssemblies == null)
            {
                lock (_assemblyCacheLock)
                {
                    if (_cachedAssemblies == null) // Double-checked locking for thread safety
                    {
                        _cachedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                            .AsParallel()
                            .Where(a =>
                                !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase) &&
                                !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) &&
                                !a.FullName.Contains("Tests", StringComparison.OrdinalIgnoreCase)) // Exclude test assemblies
                            .ToList();
                    }
                }
            }

            return includeDynamic ? _cachedAssemblies : _cachedAssemblies.Where(a => !a.IsDynamic);
        }

        public static IEnumerable<Type> GetRelevantTypes(bool includeDynamic = false)
        {
            return GetRelevantAssemblies(includeDynamic)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.Namespace?.Contains("Tests", StringComparison.OrdinalIgnoreCase) ?? true) // Exclude test namespaces
                .ToList();
        }

        public static IEnumerable<Type> FindTypesImplementingInterface<TInterface>(bool includeDynamic = false)
        {
            return GetRelevantTypes(includeDynamic)
                .Where(type => type.IsClass && !type.IsAbstract && typeof(TInterface).IsAssignableFrom(type))
                .ToList();
        }

        public static IEnumerable<Type> FindTypesDerivedFromGeneric(Type genericTypeDefinition, bool includeDynamic = false)
        {
            return GetRelevantTypes(includeDynamic)
                .Where(type => type.IsClass && !type.IsAbstract &&
                               type.BaseType is { IsGenericType: true } &&
                               type.BaseType.GetGenericTypeDefinition() == genericTypeDefinition)
                .ToList();
        }

        public static bool IsDerivedFromGenericType(Type type, Type genericType)
        {
            while (type != null && type != typeof(object))
            {
                var currentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (currentType == genericType)
                {
                    return true;
                }
                type = type.BaseType!;
            }
            return false;
        }
        
        public static string GetReadableTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetReadableTypeName));
            return $"{genericTypeName}<{genericArgs}>";
        }
    }
}
