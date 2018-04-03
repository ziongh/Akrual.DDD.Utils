using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Messaging.Saga;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation;
using Akrual.DDD.Utils.Domain.Utils.UUID;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class CommandEventWithProcessManagerTests : TestsUsingSimpleInjector
    {

    }


    public class TicketReservationProcessManagerRedirects : 
        IProcessManagerRedirect<_2OrderCreated>,
        IProcessManagerRedirect<_4SeatsReserved>,
        IProcessManagerRedirect<_6PaymentAccepted>
    {
        private Guid _userId;
        private Guid _orderId;
        private int _seatNumber;
        private Guid _reservationId;
        private Guid _paymentId;

        private double _value;
        private string _cardNumber;


        public IEnumerable<IMessaging> Redirect(_2OrderCreated message)
        {
            _userId = message.UserId;
            _orderId = message.AggregateRootId;
            _seatNumber = message.SeatNumber;
            _value = message.Value;
            _cardNumber = message.CardNumber;
            _reservationId = GuidGenerator.GenerateTimeBasedGuid();
            yield return new _3MakeReservation(_reservationId)
            {
                SeatNumber = message.SeatNumber,
                UserId = message.UserId
            };
        }

        public IEnumerable<IMessaging> Redirect(_4SeatsReserved message)
        {
            _paymentId = GuidGenerator.GenerateTimeBasedGuid();
            yield return new _5MakePayment(_paymentId)
            {
                Value = _value,
                CardNumber = _cardNumber
            };
        }

        public IEnumerable<IMessaging> Redirect(_6PaymentAccepted message)
        {
            yield return new _7OrderConfirmed()
            {
                AggregateRootId = _orderId,
                UserId = _userId,
                SeatNumber = _seatNumber
            };
        }
    }
}
