using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates
{
    public class Customer : AggregateRoot<Customer>,
        IHandleDomainEvent<_6OrderConfirmed_Customer>
    {
        public string CardNumber { get; set; }

        public Customer() : base(Guid.Empty)
        {
        }

        public async Task<IEnumerable<IMessaging>> Handle(_6OrderConfirmed_Customer notification, CancellationToken cancellationToken)
        {
            return new IMessaging[0];
        }
    }
}