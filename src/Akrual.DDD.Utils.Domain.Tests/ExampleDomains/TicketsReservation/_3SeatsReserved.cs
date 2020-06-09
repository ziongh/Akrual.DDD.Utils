using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using MessagePack;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    [MessagePackObject]
    public class _3SeatsReserved : DomainEvent
    {
        [Key(5)]
        public override string EventName { get; } = "_3SeatsReserved";

        [Key(8)]
        public int SeatNumber { get; set; }
        [Key(9)]
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