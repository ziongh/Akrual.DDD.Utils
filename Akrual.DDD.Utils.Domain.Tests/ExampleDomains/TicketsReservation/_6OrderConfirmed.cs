using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _6OrderConfirmed : DomainEvent
    {
        public int SeatNumber { get; set; }
        public Guid UserId { get; set; }

        public _6OrderConfirmed(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _6OrderConfirmed(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return SeatNumber;
            yield return UserId;
        }
    }

    public class _6OrderConfirmed_Customer : _6OrderConfirmed{
        public _6OrderConfirmed_Customer(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _6OrderConfirmed_Customer(Guid aggregateRootId) : base(aggregateRootId)
        {
        }
    }
    public class _6OrderConfirmed_Order : _6OrderConfirmed{
        public _6OrderConfirmed_Order(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _6OrderConfirmed_Order(Guid aggregateRootId) : base(aggregateRootId)
        {
        }
    }
    public class _6OrderConfirmed_Reservation : _6OrderConfirmed{
        public _6OrderConfirmed_Reservation(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _6OrderConfirmed_Reservation(Guid aggregateRootId) : base(aggregateRootId)
        {
        }
    }
}