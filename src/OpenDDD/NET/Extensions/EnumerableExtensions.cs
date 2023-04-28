using System.Collections.Generic;
using System.Linq;

namespace OpenDDD.NET.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsSubsetOf(this IEnumerable<object> enumerable, IEnumerable<object> other)
        {
            return !enumerable.Except(other).Any();
        }
    }
}
