using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Akrual.DDD.Utils.Internal.UsefulClasses
{
    public static class ReflectionHelper
    {
        public static void SetPrivatePropertyValue<T>(this T obj, string propertyName, object newValue)
        {
            var type = (Type) ((dynamic) obj).GetType();
            PropertyInfo property = type.GetProperty(propertyName);
            property.GetSetMethod(true).Invoke(obj, new object[] {newValue});
        }
    }
}
