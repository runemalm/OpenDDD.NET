using System;

namespace DDD.NETCore.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsOrIsSubType(this object theObject, object lookingForType)
        {
            var theType = theObject.GetType();
            while (theType != null)
            {
                Type genericType = theType.IsGenericType ? theType.GetGenericTypeDefinition() : theType;
                if (genericType == lookingForType)
                    return true;

                theType = theType.BaseType;
            }

            return false;
        }
    }
}
