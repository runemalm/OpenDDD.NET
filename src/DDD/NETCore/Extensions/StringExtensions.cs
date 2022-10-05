using System.Linq;

namespace DDD.NETCore.Extensions
{
    public static class StringExtension
    {
        public static bool IsTrue(this string value, bool default_ = false)
        {
            if (value == null) return default_;
            return (new object[] { "1", "true" }).Contains(value.ToLower());
        }
    }
}
