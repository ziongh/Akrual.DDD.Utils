using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _3MakeReservation : DomainCommand
    {
        public int SeatNumber { get; set; }
        public Guid UserId { get; set; }

        public _3MakeReservation(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _3MakeReservation(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return this.AggregateRootId;
            yield return this.SeatNumber;
            yield return this.UserId;
        }
    }
}