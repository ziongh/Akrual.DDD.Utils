using System;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _6PaymentAccepted : IDomainEvent
    {
        public Guid AggregateRootId { get; set; }
        public string CardNumber { get; set; }
        public double Value { get; set; }
    }
}