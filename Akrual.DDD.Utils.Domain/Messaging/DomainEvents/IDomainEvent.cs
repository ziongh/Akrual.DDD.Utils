﻿using System;
using System.Collections.Generic;
using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents
{
    /// <summary>
    /// Domain event that represents changes in a domain entity.
    /// </summary>
    public interface IDomainEvent : IHandledMessage<IEnumerable<IMessaging>>
    {
    }
}
