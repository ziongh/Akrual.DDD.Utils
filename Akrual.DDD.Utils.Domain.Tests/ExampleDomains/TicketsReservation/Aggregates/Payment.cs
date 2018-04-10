using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates
{
    public class Payment : AggregateRoot<Payment>,
        IHandleDomainCommand<_4MakePayment>
    {
        public Payment() : base(Guid.Empty)
        {
        }

        public async Task<IEnumerable<IMessaging>> Handle(_4MakePayment request, CancellationToken cancellationToken)
        {
            return HandleDomainCommand(request);
        }

        private IEnumerable<IDomainEvent> HandleDomainCommand(_4MakePayment request)
        {
            yield return new _5PaymentAccepted(request.OrderId)
            {
                CardNumber = request.CardNumber,
                Value = request.Value
            };
        }
    }
}