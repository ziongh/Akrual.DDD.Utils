using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Akrual.DDD.Utils.Domain.Entities;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Internal;

namespace Akrual.DDD.Utils.Domain.Contracts
{
    public static class Contract<TEntity>
    {
        /// <param name="entity">Entity to be evaluated</param>
        /// <param name="condition">Condition to evaluate on entity</param>
        /// <param name="userMessage">Message to be throw on evaluation failure</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static void Ensures<TException>(TEntity entity, Func<TEntity, bool> condition, string userMessage)
            where TException : ContractException
        {
            var conditionSatisfied = condition.Invoke(entity);
            if (!conditionSatisfied)
            {
                var ex = Activator.CreateInstance<TException>();
                typeof(TException).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ex, userMessage);
                throw ex;
            }
        }

        /// <param name="entity">Entity to be evaluated</param>
        /// <param name="condition">Condition to evaluate on entity</param>
        /// <param name="userMessage">Message to be throw on evaluation failure</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static void Requires<TException>(TEntity entity, Func<TEntity, bool> condition, string userMessage)
            where TException : ContractException
        {
            var conditionSatisfied = condition.Invoke(entity);
            if (!conditionSatisfied)
            {
                var ex = Activator.CreateInstance<TException>();
                typeof(TException).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ex, userMessage);
                throw ex;
            }
        }
    }

    public static class Contract
    {
        /// <param name="condition">Condition to evaluate on entity</param>
        /// <param name="userMessage">Message to be throw on evaluation failure</param>
        /// <exception cref="TException">Throw Exception if condition is not met.</exception>
        public static void Requires<TException>(bool condition, string userMessage)
            where TException : ContractException, new()
        {
            if (!condition)
            {
                Debug.WriteLine(userMessage);
                throw CreateExceptionWithMessage<TException>(userMessage);
            }
        }

        /// <param name="condition">Condition to evaluate on entity</param>
        /// <param name="userMessage">Message to be throw on evaluation failure</param>
        /// <exception cref="ContractException">Throw Exception if condition is not met.</exception>
        public static void Requires(bool condition, string userMessage)
        {
            if (!condition)
            {
                Debug.WriteLine(userMessage);
                throw CreateExceptionWithMessage<ContractException>(userMessage);
            }
        }


        public static void EnsuresNotNull(object entity, string userMessage = null)
        {
            var conditionSatisfied = entity != null;
            userMessage = userMessage ?? "Property cannot be null!";
            if (!conditionSatisfied)
            {
                throw new ContractException(userMessage);
            }
        }

        public static void EnsuresNotNullOrEmpty(string entity, string userMessage = null)
        {
            var conditionSatisfied = !string.IsNullOrEmpty(entity);
            userMessage = userMessage ?? "Property cannot be null, neither Empty!";
            if (!conditionSatisfied)
            {
                throw new ContractException(userMessage);
            }
        }

        public static void EnsuresNotNullOrEmpty(IEnumerable entity, string userMessage = null)
        {
            var conditionSatisfied = entity != null && !entity.IsNullOrEmpty();
            userMessage = userMessage ?? "Property cannot be null, neither Empty!";
            if (!conditionSatisfied)
            {
                throw new ContractException(userMessage);
            }
        }


        /// <param name="entity">Entity to be evaluated</param>
        /// <param name="condition">Condition to evaluate on entity</param>
        /// <param name="userMessage">Message to be throw on evaluation failure</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        /// <exception cref="TException">Throw Exception if condition is not met.</exception>
        public static void Ensures<TException>(object entity, Func<object, bool> condition, string userMessage)
            where TException : ContractException
        {
            var conditionSatisfied = condition.Invoke(entity);
            if (!conditionSatisfied)
            {
                throw CreateExceptionWithMessage<TException>(userMessage);
            }
        }

        /// <param name="entity">Entity to be evaluated</param>
        /// <param name="condition">Condition to evaluate on entity</param>
        /// <param name="userMessage">Message to be throw on evaluation failure</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        /// <exception cref="TException">Throw Exception if condition is not met.</exception>
        public static void Requires<TException>(object entity, Func<object, bool> condition, string userMessage)
            where TException : ContractException
        {
            var conditionSatisfied = condition.Invoke(entity);
            if (!conditionSatisfied)
            {
                throw CreateExceptionWithMessage<TException>(userMessage);
            }
        }



        private static TException CreateExceptionWithMessage<TException>(string Message) where TException : ContractException
        {
            var ex = Activator.CreateInstance<TException>();
            typeof(TException).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ex, Message);
            return ex;
        }
    }
}
