using System;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _2OrderCreated : IDomainEvent
    {
        public Guid AggregateRootId { get; set; }
        public int SeatNumber { get; set; }
        public Guid UserId { get; set; }

        public string CardNumber { get; set; }
        public double Value { get; set; }
    }
}