using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates
{
    public class Payment : AggregateRoot<Payment>,
        IHandleDomainCommand<_5MakePayment>
    {
        public Payment() : base(Guid.Empty)
        {
        }

        public async Task<IEnumerable<IDomainEvent>> Handle(_5MakePayment request, CancellationToken cancellationToken)
        {
            return HandleDomainCommand(request);
        }

        private IEnumerable<IDomainEvent> HandleDomainCommand(_5MakePayment request)
        {
            yield return new _6PaymentAccepted()
            {
                AggregateRootId = request.AggregateRootId,
                CardNumber = request.CardNumber,
                Value = request.Value
            };
        }
    }
}