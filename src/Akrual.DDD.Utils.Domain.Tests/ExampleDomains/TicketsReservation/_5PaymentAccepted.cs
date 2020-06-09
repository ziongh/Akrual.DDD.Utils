using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using MessagePack;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    [MessagePackObject]
    public class _5PaymentAccepted : DomainEvent
    {
        [Key(5)]
        public override string EventName { get; } = "_5PaymentAccepted";

        [Key(8)]
        public string CardNumber { get; set; }
        [Key(9)]
        public double Value { get; set; }


        public _5PaymentAccepted(Guid eventId,Guid aggregateRootId) : base(eventId,aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return CardNumber;
            yield return Value;
        }
    }
}