using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _5PaymentAccepted : DomainEvent
    {
        public string CardNumber { get; set; }
        public double Value { get; set; }

        public _5PaymentAccepted(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _5PaymentAccepted(Guid aggregateRootId) : base(aggregateRootId)
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