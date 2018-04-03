using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _5MakePayment : DomainCommand
    {
        public string CardNumber { get; set; }
        public double Value { get; set; }
        public _5MakePayment(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _5MakePayment(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return this.AggregateRootId;
            yield return this.CardNumber;
            yield return this.Value;
        }
    }
}