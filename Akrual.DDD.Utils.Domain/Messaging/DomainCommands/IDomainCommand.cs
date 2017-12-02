using System;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>
    /// Domain event that represents changes in a domain entity.
    /// </summary>
    public interface IDomainCommand : IMessaging
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        Guid AggregateRootId { get; }
    }
}
