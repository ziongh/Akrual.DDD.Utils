using System;
using System.Reflection;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Domain.Factories.InstanceFactory
{
    public class StubbedInstantiator<T> : IInstantiator<T>  where T : class
    {
        private readonly Func<T> _entity;
        public StubbedInstantiator(Func<T> entity)
        {
            _entity = entity;
        }
        public T Create(Guid id)
        {
            var instance = _entity.Invoke();
            instance.SetPrivatePropertyValue("Id", id);
            return instance;
        }
    }
}