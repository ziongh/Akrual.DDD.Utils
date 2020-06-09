using System;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents.SpecialEvents
{
    /// <summary>
    /// Represents an Event That Will Undo another Event that already happened.
    /// The EventGuid property will point to the event already happened. 
    /// And the Event Property will be the event data to undo the other event.
    /// </summary>
    public interface IUndoEvent
    {
        /// <summary>
        /// The event data to undo the other event.
        /// </summary>
        IDomainEvent Event { get; set; }

        /// <summary>
        /// Points to the event already happened.
        /// </summary>
        Guid EventGuid { get; set; }
    }
}