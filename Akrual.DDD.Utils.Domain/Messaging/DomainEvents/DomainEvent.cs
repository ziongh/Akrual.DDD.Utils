using System;

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
        /// <param name="aggregateRootId">Entity instance id.</param>
        protected DomainEvent(Guid aggregateRootId)
        {
            if (aggregateRootId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Entity id must be defined.", "aggregateRootId");
            }

            AggregateRootId = aggregateRootId;
        }
    }
}
