using System;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents
{
    /// <summary>
    /// Base class for implementing domain events that represent changes in a domain entity.
    /// </summary>
    public abstract class DomainEvent : BaseMessaging<DomainEvent>, IDomainEvent
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        public Guid AggregateRootId { get; private set; }

        /// <summary>
        /// Defines the Event Name that will be used in the Store.
        /// It is important to keep this name the same. Because, otherwise, 
        /// there will be some problems when fetching the streams.
        /// </summary>
        public abstract string EventName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="eventGuid">Event id.</param>
        /// <param name="aggregateRootId">Entity instance id.</param>
        /// <param name="sagaId">Saga Id to identify WorkFlow.</param>
        protected DomainEvent(Guid eventGuid, Guid aggregateRootId, Guid sagaId)
        {
            if (aggregateRootId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Entity id must be defined.", "aggregateRootId");
            } 
            if (eventGuid.Equals(Guid.Empty))
            {
                throw new ArgumentException("Event id must be defined.", "eventGuid");
            }

            EventGuid = eventGuid;
            AggregateRootId = aggregateRootId;
            SagaId = sagaId;
            TimeStamp = DateTimeProvider.Current.UtcNow;
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="eventGuid">Event id.</param>
        /// <param name="aggregateRootId">Entity instance id.</param>
        protected DomainEvent(Guid eventGuid, Guid aggregateRootId):this(eventGuid,aggregateRootId,Guid.NewGuid())
        {
        }

        public Guid EventGuid { get; set; }

        public DateTime? AppliesAt { get; set; }

        public DateTime TimeStamp { get; protected set; }
        public Guid SagaId { get; protected set; }
    }
}
