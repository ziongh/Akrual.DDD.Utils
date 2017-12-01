using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Internal.Contracts;

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
    }
}
