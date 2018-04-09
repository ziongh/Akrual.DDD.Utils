using System;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates
{
    public class Customer : AggregateRoot<Customer>//,
        //IHandleDomainEvent<_7OrderConfirmed>
    {
        public string CardNumber { get; set; }

        public Customer() : base(Guid.Empty)
        {
        }

        public async Task Handle(_7OrderConfirmed notification, CancellationToken cancellationToken)
        {

        }
    }
}