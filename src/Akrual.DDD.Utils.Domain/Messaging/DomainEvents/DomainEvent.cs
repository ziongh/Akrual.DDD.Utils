using System;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using MessagePack;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents
{
    /// <summary>
    /// Base class for implementing domain events that represent changes in a domain entity.
    /// </summary>
    [MessagePackObject]
    public abstract class DomainEvent : BaseMessaging<DomainEvent>, IDomainEvent
    {
        [Key(0)]
        public Guid EventGuid { get; set; }

        /// <summary>
        /// Gets the entity id.
        /// </summary>
        [Key(1)]
        public Guid AggregateRootId { get; private set; }


        /// <summary>
        /// Defines the Event Name that will be used in the Store.
        /// It is important to keep this name the same. Because, otherwise, 
        /// there will be some problems when fetching the streams.
        /// </summary>
        [MessagePack.IgnoreMember]
        public abstract string EventName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="eventGuid">Event id.</param>
        /// <param name="aggregateRootId">Entity instance id.</param>
        /// <param name="sagaId">Saga Id to identify WorkFlow.</param>
        [SerializationConstructor]
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


        [Key(6)]
        public DateTime? AppliesAt { get; set; }

        [Key(7)]
        public DateTime TimeStamp { get; protected set; }
    }
}
