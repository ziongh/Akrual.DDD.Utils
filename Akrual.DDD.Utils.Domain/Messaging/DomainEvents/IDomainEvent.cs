using System;
using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents
{
    /// <summary>
    /// Domain event that represents changes in a domain entity.
    /// </summary>
    public interface IDomainEvent : IMessaging, INotification
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        Guid AggregateRootId { get; }
    }
}
