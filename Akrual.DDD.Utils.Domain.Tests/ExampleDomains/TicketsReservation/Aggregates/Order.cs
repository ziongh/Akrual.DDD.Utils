using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates
{
    public class Order : AggregateRoot<Order>,
        IHandleDomainCommand<_1PlaceOrder>,
        IHandleDomainEvent<_7OrderConfirmed>
    {
        public int SeatNumber { get; private set; }
        public Guid UserId { get; private set; }

        public Order() : base(Guid.Empty)
        {
        }

        public async Task<IEnumerable<IDomainEvent>> Handle(_1PlaceOrder request, CancellationToken cancellationToken)
        {
            return HandleDomainCommand(request);
        }

        private IEnumerable<IDomainEvent> HandleDomainCommand(_1PlaceOrder request)
        {
            yield return new _2OrderCreated()
            {
                AggregateRootId = request.AggregateRootId,
                SeatNumber = request.SeatNumber,
                UserId = request.UserId,
                Value = request.Value,
                CardNumber = request.CardNumber
            };
        }

        public async Task Handle(_7OrderConfirmed notification, CancellationToken cancellationToken)
        {
            this.SeatNumber = notification.SeatNumber;
            this.UserId = notification.UserId;
        }
    }
}