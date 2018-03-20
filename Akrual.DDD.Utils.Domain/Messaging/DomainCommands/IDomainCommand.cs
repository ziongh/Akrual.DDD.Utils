using System;
using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>
    /// Domain Command that represents the will for something to happen
    /// </summary>
    public interface IDomainCommand<out TResponse> : IMessaging, IRequest<TResponse>
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        Guid AggregateRootId { get; }
    }
}
