using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Akrual.DDD.Utils.Internal.Exceptions;
using Akrual.DDD.Utils.Internal.Extensions;

namespace Akrual.DDD.Utils.Internal.Contracts
{
    /// <summary>
    /// Contract Helper class to check simple conditions in a easy way
    /// </summary>
    public static class CommonContract
    {
        public static void Ensures<T>(this T attemptedValue, Func<T, bool> condition, string message = "", ContractException ex = null)
        {
            condition.EnsuresNotNull();
            var conditionSatisfied = condition.Invoke(attemptedValue);
            if (!conditionSatisfied)
            {
                ex = ex ?? new ContractExceptionWithProperty(message ?? $"Argument did not satisfied Condition.")
                {
                    AttemptedValue = attemptedValue
                };
                throw ex;
            }
        }

        public static void EnsuresNotNull<T>(this T attemptedValue, string message = "", ContractException ex = null)
        {
            var conditionSatisfied = attemptedValue != null;
            if (!conditionSatisfied)
            {
                ex = ex ?? new ContractExceptionWithProperty(message ?? $"Argument did not satisfied Condition.")
                {
                    AttemptedValue = attemptedValue
                };
                throw ex;
            }
        }

        public static void EnsuresNotNullOrEmpty(this string attemptedValue, string userMessage = null, ContractExceptionWithProperty ex = null)
        {
            var conditionSatisfied = !string.IsNullOrEmpty(attemptedValue);
            userMessage = userMessage ?? "Property cannot be null, neither Empty!";
            if (!conditionSatisfied)
            {
                ex = ex ?? new ContractExceptionWithProperty(userMessage)
                {
                    AttemptedValue = attemptedValue
                };
                throw ex;
            }
        }

        public static void EnsuresNotNullOrEmpty(this IEnumerable attemptedValue, string userMessage = null, ContractExceptionWithProperty ex = null)
        {
            var conditionSatisfied = attemptedValue != null && !attemptedValue.IsNullOrEmpty();
            userMessage = userMessage ?? "Property cannot be null, neither Empty!";
            if (!conditionSatisfied)
            {
                ex = ex ?? new ContractExceptionWithProperty(userMessage)
                {
                    AttemptedValue = attemptedValue
                };
                throw ex;
            }
        }

        public static void EnsuresNotNullOrEmpty<T>(this IEnumerable<T> attemptedValue, string userMessage = null, ContractExceptionWithProperty ex = null)
        {
            var conditionSatisfied = attemptedValue != null && attemptedValue.Any();
            userMessage = userMessage ?? "Property cannot be null, neither Empty!";
            if (!conditionSatisfied)
            {
                ex = ex ?? new ContractExceptionWithProperty(userMessage)
                {
                    AttemptedValue = attemptedValue
                };
                throw ex;
            }
        }


        /// <summary>
        /// Value type must be of specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attemptedValue"></param>
        public static void EnsuresOfType<T>(this object attemptedValue)
        {
            var tp = typeof(T);
            var ex = false;
            if (attemptedValue == null)
            {
#if COREFX
                if (tp.GetTypeInfo().IsClass) return;
#else
                if (tp.IsClass) return;
#endif

                ex = true;
            }
            if (ex || (attemptedValue.GetType() != tp)) throw new ContractExceptionWithProperty($"Argument must be of type '{tp}'")
            {
                AttemptedValue = attemptedValue.GetType(),
                ExpectedValue = tp
            };
        }


        public static void EnsuresEqualTo<T>(this T attemptedValue, T expectedValue, string msg = "", ContractExceptionWithProperty ex = null) where T : IEquatable<T>
            => attemptedValue.Ensures(d => d.Equals(expectedValue), msg, ex);

        public static void EnsuresDiferrentThan<T>(this T attemptedValue, T expectedValue, string msg = "", ContractExceptionWithProperty ex = null) where T : IEquatable<T>
            => attemptedValue.Ensures(d => !d.Equals(expectedValue), msg, ex);

        /// <summary>
        /// Arugment must implement interface T
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="value"></param>
        public static void EnsuresImplement<T>(this object value)
        {
            value.EnsuresNotNull("value");
            var tp = typeof(T);
#if COREFX
            if (!tp.GetTypeInfo().IsInterface) throw new ArgumentException($"'{tp}' is not an interface");
#else
            if (!tp.IsInterface) throw new ArgumentException($"'{tp}' is not an interface");
#endif
            var otype = value.GetType();

            if (value is Type)
            {
                otype = value as Type;
            }

            if (!otype.Implements(tp)) throw new ArgumentException($"Argument must implement '{tp}'");
        }

    }
}
