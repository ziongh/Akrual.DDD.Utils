using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _1PlaceOrder : DomainCommand
    {
        public int SeatNumber { get; set; }
        public Guid UserId { get; set; }
        public string CardNumber { get; set; }
        public double Value { get; set; }

        public _1PlaceOrder(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _1PlaceOrder(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return this.AggregateRootId;
            yield return this.SeatNumber;
            yield return this.UserId;
            yield return this.CardNumber;
            yield return this.Value;
        }
    }
}