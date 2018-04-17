using System;
using System.Reflection;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Akrual.DDD.Utils.Domain.Factories.InstanceFactory
{
    /// <summary>
    /// This is a dangerous Class. Because it creates one Async Scope just to create a new instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Instantiator<T> : IInstantiator<T> where T : class
    {
        private readonly Container _container;
        public Instantiator(Container container)
        {
            _container = container;
        }

        /// <summary>
        /// <remarks>Dangerous!</remarks> Because it creates one new async scope to create a new instance.
        /// </summary>
        /// <returns></returns>
        public T Create(Guid id)
        {
            using (AsyncScopedLifestyle.BeginScope(_container))
            {
                T instance = _container.GetInstance<T>();
                instance.SetPrivatePropertyValue("Id", id);
                return instance;
            }
        }
    }
}