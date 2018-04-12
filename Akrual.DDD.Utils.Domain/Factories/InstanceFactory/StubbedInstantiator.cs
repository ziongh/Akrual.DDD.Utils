using System;

namespace Akrual.DDD.Utils.Domain.Factories.InstanceFactory
{
    public class StubbedInstantiator<T> : IInstantiator<T>  where T : class
    {
        private readonly Func<T> _entity;
        public StubbedInstantiator(Func<T> entity)
        {
            _entity = entity;
        }
        public T Create()
        {
            return _entity.Invoke();
        }
    }
}