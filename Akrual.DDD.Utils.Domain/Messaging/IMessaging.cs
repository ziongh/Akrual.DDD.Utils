using System;
using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging
{
    /// <summary>
    /// Messages (and not objects, objects are data + behavior).
    /// In some respects much like DTOs, they communicate data 
    /// about an event and they themselves encapsulate no behavior
    /// </summary>
    public interface IMessaging
    {
        DateTime TimeStamp { get;}
        Guid SagaId { get; }
    }


    /// <summary>
    /// Represents Any message that will be used to represent a Command or an Event.
    /// </summary>
    public interface IHandledMessage<out TResponse> : IMessaging, IRequest<TResponse>
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        Guid AggregateRootId { get; }
    }
}
