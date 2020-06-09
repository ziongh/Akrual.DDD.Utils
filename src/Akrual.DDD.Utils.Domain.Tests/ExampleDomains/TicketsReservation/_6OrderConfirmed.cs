using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using MessagePack;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    [MessagePackObject]
    [Union(0, typeof(_6OrderConfirmed_Customer))]
    [Union(1, typeof(_6OrderConfirmed_Order))]
    [Union(2, typeof(_6OrderConfirmed_Reservation))]
    public abstract class _6OrderConfirmed : DomainEvent
    {
        [Key(8)]
        public int SeatNumber { get; set; }
        [Key(9)]
        public Guid UserId { get; set; }


        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return SeatNumber;
            yield return UserId;
        }

        protected _6OrderConfirmed(Guid eventId,Guid aggregateRootId) : base(eventId,aggregateRootId)
        {
        }
    }

    [MessagePackObject]
    public class _6OrderConfirmed_Customer : _6OrderConfirmed
    {
        [Key(5)]
        public override string EventName { get; } = "_6OrderConfirmed_Customer";

        public _6OrderConfirmed_Customer(Guid eventId,Guid aggregateRootId) : base(eventId,aggregateRootId)
        {
        }

    }

    [MessagePackObject]
    public class _6OrderConfirmed_Order : _6OrderConfirmed
    {
        [Key(5)]
        public override string EventName { get; } = "_6OrderConfirmed_Order";

        public _6OrderConfirmed_Order(Guid eventId,Guid aggregateRootId) : base(eventId,aggregateRootId)
        {
        }
    }

    [MessagePackObject]
    public class _6OrderConfirmed_Reservation : _6OrderConfirmed
    {
        [Key(5)]
        public override string EventName { get; } = "_6OrderConfirmed_Reservation";

        public _6OrderConfirmed_Reservation(Guid eventId,Guid aggregateRootId) : base(eventId,aggregateRootId)
        {
        }
    }
}