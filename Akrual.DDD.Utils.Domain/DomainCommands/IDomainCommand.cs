using System;

namespace Akrual.DDD.Utils.Domain.DomainCommands
{
    /// <summary>
    /// Domain event that represents changes in a domain entity.
    /// </summary>
    public interface IDomainCommand
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        Guid EntityId { get; }
    }
}
