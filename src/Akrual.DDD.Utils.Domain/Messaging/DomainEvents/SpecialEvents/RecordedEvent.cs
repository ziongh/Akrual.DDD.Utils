﻿using System;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents.SpecialEvents
{
    /// <summary>
    /// Represents an Event That is recorded into the event Store. It has a new property that
    /// indicates the exact time the event was inserted into the stream.
    /// </summary>
    public class RecordedEvent : IRecordedEvent
    {
        /// <summary>
        /// The recorded Event.
        /// </summary>
        public IDomainEvent Event { get; set; }
        /// <summary>
        /// The date and time in which the event was stored.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}