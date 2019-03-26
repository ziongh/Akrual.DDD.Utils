using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.Buses;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Utils.UUID;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates
{
    public class Reservation : AggregateRoot<Reservation>,
        IHandleDomainCommand<_2MakeReservation>,
        IHandleDomainEvent<_6OrderConfirmed_Reservation>
    {
        public Reservation(IBus bus) : base(Guid.Empty,bus)
        {
        }
        
        public async Task<IEnumerable<IMessaging>> Handle(_2MakeReservation request, CancellationToken cancellationToken)
        {
            return HandleDomainCommand(request);
        }

        private IEnumerable<IDomainEvent> HandleDomainCommand(_2MakeReservation request)
        {
            yield return new _3SeatsReserved(GuidGenerator.GenerateTimeBasedGuid(),request.OderId)
            {
                UserId = request.UserId,
                SeatNumber = request.SeatNumber
            };
        }
        

        public async Task<IEnumerable<IMessaging>> Handle(_6OrderConfirmed_Reservation notification, CancellationToken cancellationToken)
        {
            return new IMessaging[0];
        }

    }
}