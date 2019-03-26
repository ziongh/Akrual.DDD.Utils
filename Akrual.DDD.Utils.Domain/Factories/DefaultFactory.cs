using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.Buses;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.UOW;

namespace Akrual.DDD.Utils.Domain.Factories
{
    /// <summary>
    /// This class implements the IFactory Interface and the FactoryBase Abstract class.
    /// The behaviour: The Create method will call new() and then will set the Id Property to the given guid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultFactory<T> : FactoryBase<T> where T : class, IAggregateRoot
    {
        private readonly IUnitOfWork _uow;
        private readonly IInstantiator<T> _instantiator;
        private readonly Type _type;

        public DefaultFactory(IUnitOfWork uow, IInstantiator<T> instantiator)
        {
            _uow = uow;
            _instantiator = instantiator;
            _type = (Type) ((dynamic) _instantiator.Create(Guid.Empty)).GetType();
        }


        public override async Task<T> Create(Guid guid)
        {
            var loadedEntity = _uow.GetLoadedAggregate(_type, guid);
            if (loadedEntity != null)
            {
                return (T)loadedEntity;
            }

            T created = await base.Create(guid);

            created = (T)_uow.AddLoadedAggregate(created);
            return created;
        }


        protected internal override async Task<T> CreateDefaultInstance(Guid guid)
        {
            T result = _instantiator.Create(guid);
            return result;
        }
    }
}