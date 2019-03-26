using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Akrual.DDD.Utils.Domain.Messaging.Buses;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Domain.Factories.InstanceFactory
{
    public class SimpleInstantiator<T> : IInstantiator<T> where T : class
    {
        public T Create(Guid id)
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<T>();
            T instance = (T) Activator.CreateInstance(typeof(T),bus);
            instance.SetPrivatePropertyValue("Id", id);
            return instance;
        }
    }
}
