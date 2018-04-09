using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates
{
    public class Reservation : AggregateRoot<Reservation>,
        IHandleDomainCommand<_3MakeReservation>//,
        //IHandleDomainEvent<_7OrderConfirmed>
    {
        public Reservation() : base(Guid.Empty)
        {
        }
        
        public async Task<IEnumerable<IDomainEvent>> Handle(_3MakeReservation request, CancellationToken cancellationToken)
        {
            return HandleDomainCommand(request);
        }

        private IEnumerable<IDomainEvent> HandleDomainCommand(_3MakeReservation request)
        {
            yield return new _4SeatsReserved()
            {
                AggregateRootId = request.AggregateRootId,
                UserId = request.UserId,
                SeatNumber = request.SeatNumber
            };
        }
        

        public async Task Handle(_7OrderConfirmed notification, CancellationToken cancellationToken)
        {
        }

    }
}