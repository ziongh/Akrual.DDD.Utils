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
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="aggregateRootId">Entity instance id.</param>
        /// <param name="entityVersion">Entity instance version.</param>
        protected DomainEvent(Guid aggregateRootId, long entityVersion)
        {
            if (aggregateRootId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Entity id must be defined.", "aggregateRootId");
            }

            AggregateRootId = aggregateRootId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="aggregateRootId">Entity instance id.</param>
        protected DomainEvent(Guid aggregateRootId)
            : this(aggregateRootId, 0)
        {
        }
    }
}
