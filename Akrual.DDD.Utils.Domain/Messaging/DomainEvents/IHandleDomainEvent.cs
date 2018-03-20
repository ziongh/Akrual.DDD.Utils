﻿using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents
{
    /// <summary>
    /// Implemented by an aggregate once for each event type it can apply.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IHandleDomainEvent<in TEvent> : INotificationHandler<TEvent> where TEvent : IDomainEvent
    {
    }
}
