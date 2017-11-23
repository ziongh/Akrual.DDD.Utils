using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.DomainEvents;
using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Aggregates
{
    /// <summary>
    ///     Base class for implementing aggregate root domain objects.
    ///     <remarks><c>No external code should access the internal objects of this Aggregate!</c></remarks>
    ///     <remarks><c>So add properties as internal only!</c></remarks>
    /// </summary>
    public abstract class AggregateRoot<T> : Entity<T>
    {
        private readonly List<DomainEvent> appliedEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
        /// </summary>
        /// <param name="id">Aggregate root instance id.</param>
        protected AggregateRoot(Guid id)
        {
            if (id == default(Guid))
            {
                throw new ArgumentException("Id must be defined.", "id");
            }

            Id = id;
            appliedEvents = new List<DomainEvent>();
        }
   
        /// <summary>
        /// Gets all domain events that have been applied to the aggregate root instance.
        /// </summary>
        /// <returns>A collection of domain events.</returns>
        public virtual IEnumerable<IDomainEvent> GetAppliedEvents()
        {
            return appliedEvents;
        }

        /// <summary>
        /// Applies a new domain event to the aggregate root instance.
        /// </summary>
        /// <param name="domainEvent">The new domain event to apply.</param>
        protected void ApplyEvent(DomainEvent domainEvent)
        {
            appliedEvents.Add(domainEvent);
        }
    }
}
