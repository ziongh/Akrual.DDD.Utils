using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Domain.Factories.InstanceFactory
{
    public class SimpleInstantiator<T> : IInstantiator<T> where T : class
    {
        public T Create(Guid id)
        {
            T instance = Activator.CreateInstance<T>();
            instance.SetPrivatePropertyValue("Id", id);
            return instance;
        }
    }
}
