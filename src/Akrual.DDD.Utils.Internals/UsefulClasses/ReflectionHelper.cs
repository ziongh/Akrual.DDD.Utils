using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool IsTheGenericType(this Type candidateType, Type genericType)
        {
            return
                candidateType != null && genericType != null &&
                (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == genericType ||
                 candidateType.GetInterfaces()
                     .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType) ||
                 candidateType.BaseType != null && candidateType.BaseType.IsTheGenericType(genericType));
        }

        public static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

    }
}
