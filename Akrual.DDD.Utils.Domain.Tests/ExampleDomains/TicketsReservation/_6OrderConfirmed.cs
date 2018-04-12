using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public abstract class _6OrderConfirmed : DomainEvent
    {
        public int SeatNumber { get; set; }
        public Guid UserId { get; set; }


        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return SeatNumber;
            yield return UserId;
        }

        protected _6OrderConfirmed(Guid aggregateRootId) : base(aggregateRootId)
        {
        }
    }

    public class _6OrderConfirmed_Customer : _6OrderConfirmed
    {
        public override string EventName { get; } = "_6OrderConfirmed_Customer";

        public _6OrderConfirmed_Customer(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

    }
    public class _6OrderConfirmed_Order : _6OrderConfirmed
    {
        public override string EventName { get; } = "_6OrderConfirmed_Order";

        public _6OrderConfirmed_Order(Guid aggregateRootId) : base(aggregateRootId)
        {
        }
    }
    public class _6OrderConfirmed_Reservation : _6OrderConfirmed
    {
        public override string EventName { get; } = "_6OrderConfirmed_Reservation";

        public _6OrderConfirmed_Reservation(Guid aggregateRootId) : base(aggregateRootId)
        {
        }
    }
}