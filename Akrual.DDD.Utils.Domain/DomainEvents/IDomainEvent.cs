using System;

namespace Akrual.DDD.Utils.Domain.DomainEvents
{
    /// <summary>
    /// Domain event that represents changes in a domain entity.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        Guid EntityId { get; }
    }
}
