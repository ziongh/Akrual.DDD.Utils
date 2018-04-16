using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _3SeatsReserved : DomainEvent
    {
        public override string EventName { get; } = "_3SeatsReserved";

        public int SeatNumber { get; set; }
        public Guid UserId { get; set; }


        public _3SeatsReserved(Guid eventId,Guid aggregateRootId) : base(eventId,aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return SeatNumber;
            yield return UserId;
        }
    }
}