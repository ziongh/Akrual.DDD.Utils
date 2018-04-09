using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.UOW;

namespace Akrual.DDD.Utils.Domain.Factories
{
    /// <summary>
    /// This class implements the IDefaultFactory Interface and the Factory Abstract class.
    /// The behaviour: The Create method will call new() and then will set the Id Property to the given guid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultFactory<T> : Factory<T, T>, IDefaultFactory<T> where T : IAggregateRoot
    {
        private readonly IUnitOfWork _uow;
        private readonly Func<T> _instanceFactory;
        private readonly Type _type;

        public DefaultFactory(IUnitOfWork uow, Func<T> instanceFactory)
        {
            _uow = uow;
            _instanceFactory = instanceFactory;
            _type = (Type) ((dynamic) _instanceFactory.Invoke()).GetType();
        }

        public override async Task<T> Create(Guid guid)
        {
            var loadedEntity = _uow.GetLoadedAggregate(_type, guid);
            if (loadedEntity != null)
            {
                return (T) loadedEntity;
            }

            T created = await base.Create(guid);

            created = (T) _uow.AddLoadedAggregate(created);
            return created;
        }

        public override async Task<T> CreateDefaultInstance(Guid guid)
        {
            T result = _instanceFactory.Invoke();
            SetPrivatePropertyValue(result, "Id", guid);
            return result;
        }

        public static void SetPrivatePropertyValue<T>(T obj, string propertyName, object newValue)
        {
            var type = (Type) ((dynamic) obj).GetType();
            PropertyInfo property = type.GetProperty(propertyName);
            property.GetSetMethod(true).Invoke(obj, new object[] {newValue});
        }
    }
}