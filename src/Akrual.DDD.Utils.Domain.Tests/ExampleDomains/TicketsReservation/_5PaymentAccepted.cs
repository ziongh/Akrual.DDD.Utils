using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _5PaymentAccepted : DomainEvent
    {
        public override string EventName { get; } = "_5PaymentAccepted";

        public string CardNumber { get; set; }
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