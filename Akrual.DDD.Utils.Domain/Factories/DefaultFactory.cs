using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.UOW;

namespace Akrual.DDD.Utils.Domain.Factories
{
    /// <summary>
    /// This class implements the IDefaultFactory Interface and the Factory Abstract class.
    /// The behaviour: The Create method will call new() and then will set the Id Property to the given guid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultFactory<T> : Factory<T, T>, IDefaultFactory<T> where T : class, IAggregateRoot
    {
        private readonly IUnitOfWork _uow;
        private readonly IEventStore _eventStore;
        private readonly IInstantiator<T> _instantiator;
        private readonly Type _type;

        public DefaultFactory(IUnitOfWork uow, IInstantiator<T> instantiator, IEventStore eventStore)
        {
            _uow = uow;
            _eventStore = eventStore;
            _instantiator = instantiator;
            _type = (Type) ((dynamic) _instantiator.Create()).GetType();
        }

        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order. Then it check all later events that should be applied before the given
        ///     point and applies them too.
        ///     <remarks> 
        ///         In other words, it will give the result of a particular time point within the stream (everything we know 
        ///         that happened until that point in time, counting with events that may have happened later, but affects some point back in time) 
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public override async Task<T> CreateAsOf(Guid guid, DateTime? AsOfDate = null)
        {
            var loadedEntity = _uow.GetLoadedAggregate(_type, guid);
            if (loadedEntity != null)
            {
                return (T) loadedEntity;
            }

            T created = await base.CreateAsOf(guid, AsOfDate);

            created = (T) _uow.AddLoadedAggregate(created);
            return created;
        }

        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order.
        ///     <remarks> 
        ///         In other words, it will give the result at a particular time point within the stream (what did we know as at this point in time) 
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public override async Task<T> CreateAsAt(Guid guid, DateTime? AsAtDate = null)
        {
            var loadedEntity = _uow.GetLoadedAggregate(_type, guid);
            if (loadedEntity != null)
            {
                return (T) loadedEntity;
            }

            T created = await base.CreateAsAt(guid, AsAtDate);

            created = (T) _uow.AddLoadedAggregate(created);
            return created;
        }

        protected internal override async Task<T> CreateDefaultInstanceAsOf(Guid guid, DateTime? AsOfDate = null)
        {
            T result = _instantiator.Create();
            SetPrivatePropertyValue(result, "Id", guid);


            result = await RebuildAggregateFromEventStreamAsOf(result,AsOfDate);


            return result;
        }

        protected internal override async Task<T> CreateDefaultInstanceAsAt(Guid guid, DateTime? AsAtDate = null)
        {
            T result = _instantiator.Create();
            SetPrivatePropertyValue(result, "Id", guid);


            result = await RebuildAggregateFromEventStreamAsAt(result,AsAtDate);


            return result;
        }

        private async Task<T> RebuildAggregateFromEventStreamAsAt(T result, DateTime? AsAtDate = null)
        {
            var stream = await _eventStore.GetEventStream(result);
            
            foreach (var recordedEvent in await stream.Events)
            {
                var @event = recordedEvent.Event;
                if (AsAtDate == null || recordedEvent.CreatedAt <= AsAtDate)
                {
                    await result.ApplyOneEvent((dynamic) @event, new Aggregates.Internal());
                }
            }

            result.AllEventsStored();
            return result;
        }

        private async Task<T> RebuildAggregateFromEventStreamAsOf(T result, DateTime? AsOfDate = null)
        {
            var stream = await _eventStore.GetEventStream(result);
            
            foreach (var recordedEvent in await stream.Events)
            {
                var @event = recordedEvent.Event;
                if (AsOfDate == null || recordedEvent.CreatedAt <= AsOfDate)
                {
                    await result.ApplyOneEvent((dynamic) @event, new Aggregates.Internal());
                }
                else if (@event.AppliesAt <= AsOfDate)
                {
                    await result.ApplyOneEvent((dynamic) @event, new Aggregates.Internal());
                }
            }

            result.AllEventsStored();
            return result;
        }


        private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object newValue)
        {
            var type = (Type) ((dynamic) obj).GetType();
            PropertyInfo property = type.GetProperty(propertyName);
            property.GetSetMethod(true).Invoke(obj, new object[] {newValue});
        }
    }
}