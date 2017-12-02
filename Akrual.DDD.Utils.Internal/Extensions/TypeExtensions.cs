using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Akrual.DDD.Utils.Internal.Contracts;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Internal.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Used for checking if a class implements an interface
        /// </summary>
        /// <param name="type">Class Implementing the interface</param>
        /// <param name="interfaceType">Type of an interface</param>
        /// <returns></returns>
        public static bool Implements(this Type type, Type interfaceType)
        {
            type.EnsuresNotNull();
            interfaceType.EnsuresNotNull();
#if COREFX
            if (!interfaceType.GetTypeInfo().IsInterface) throw new ArgumentException($"The generic type '{interfaceType}' is not an interface");
#else
            if (!interfaceType.IsInterface) throw new ArgumentException($"The generic type '{interfaceType}' is not an interface");
#endif
            return interfaceType.IsAssignableFrom(type);
        }



        public static bool PublicInstancePropertiesEqual<T>(this T actual, T expected, ref List<DiferentProperty> diferentProperties, params string[] ignore) where T : class
        {
            diferentProperties = new List<DiferentProperty>();
            bool ok = true;
            if (actual == null ||
                expected == null)
            {
                return actual == expected;
            }

            Type type = typeof(T);
            List<string> ignoreList = new List<string>(ignore);
            foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (ignoreList.Contains(pi.Name))
                {
                    continue;
                }
                object actualValue = type.GetProperty(pi.Name).GetValue(actual, null);
                object expectedValue = type.GetProperty(pi.Name).GetValue(expected, null);

                if ((actualValue != expectedValue && (actualValue == null || !actualValue.Equals(expectedValue))))
                {
                    diferentProperties.Add(new DiferentProperty()
                    {
                        PropertyName = pi.Name,
                        PropertyActualValue = actualValue != null ? actualValue.ToString() : "NULL",
                        PropertyExpectedValue = expectedValue != null ? expectedValue.ToString() : "NULL"
                    });
                    ok = false;
                }
            }
            return ok;
        }
    }
}
