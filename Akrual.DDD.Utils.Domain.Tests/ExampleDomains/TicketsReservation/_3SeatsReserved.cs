﻿using System;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation
{
    public class _3SeatsReserved : DomainEvent
    {
        public int SeatNumber { get; set; }
        public Guid UserId { get; set; }

        public _3SeatsReserved(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public _3SeatsReserved(Guid aggregateRootId) : base(aggregateRootId)
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