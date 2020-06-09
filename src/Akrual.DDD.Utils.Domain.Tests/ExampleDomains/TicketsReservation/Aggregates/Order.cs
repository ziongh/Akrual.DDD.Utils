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
    public class Order : AggregateRoot<Order>,
        IHandleDomainCommand<_1PlaceOrder>,
        IHandleDomainEvent<_3SeatsReserved>,
        IHandleDomainEvent<_5PaymentAccepted>,
        IHandleDomainEvent<_6OrderConfirmed_Order>
    {
        public override string StreamBaseName => "Order";
        public int SeatNumber { get; private set; }
        public Guid UserId { get; private set; }




        private Guid _userId;
        private int _seatNumber;
        private Guid _reservationId;
        private Guid _paymentId;

        private double _value;
        private string _cardNumber;

        public Order(IBus bus) : base(Guid.Empty,bus)
        {
        }
        public async Task<IEnumerable<IMessaging>> Handle(_1PlaceOrder request, CancellationToken cancellationToken)
        {
            return HandlePlaceOrder(request);
        }

        private IEnumerable<IMessaging> HandlePlaceOrder(_1PlaceOrder request)
        {
            _userId = request.UserId;
            _seatNumber = request.SeatNumber;
            _value = request.Value;
            _cardNumber = request.CardNumber;
            _reservationId = GuidGenerator.GenerateTimeBasedGuid();
            yield return new _2MakeReservation(_reservationId)
            {
                SeatNumber = request.SeatNumber,
                UserId = request.UserId,
                OderId = this.Id
            };
        }

        public async Task<IEnumerable<IMessaging>> Handle(_6OrderConfirmed_Order notification, CancellationToken cancellationToken)
        {
            this.SeatNumber = notification.SeatNumber;
            this.UserId = notification.UserId;
            return new IMessaging[0];
        }

        public async Task<IEnumerable<IMessaging>> Handle(_3SeatsReserved request, CancellationToken cancellationToken)
        {
            _paymentId = GuidGenerator.GenerateTimeBasedGuid();
            return HandleSeatReserved(request);
        }

        private IEnumerable<IMessaging> HandleSeatReserved(_3SeatsReserved request)
        {
            yield return new _4MakePayment(_paymentId)
            {
                Value = _value,
                CardNumber = _cardNumber,
                OrderId = this.Id
            };
        }

        public async Task<IEnumerable<IMessaging>> Handle(_5PaymentAccepted request, CancellationToken cancellationToken)
        {
            return HandlePaymentAccepted(request);
        }

        private IEnumerable<IMessaging> HandlePaymentAccepted(_5PaymentAccepted request)
        {
            yield return new _6OrderConfirmed_Customer(GuidGenerator.GenerateTimeBasedGuid(),_userId)
            {
                UserId = _userId,
                SeatNumber = _seatNumber
            };
            yield return new _6OrderConfirmed_Order(GuidGenerator.GenerateTimeBasedGuid(),this.Id)
            {
                UserId = _userId,
                SeatNumber = _seatNumber
            };
            yield return new _6OrderConfirmed_Reservation(GuidGenerator.GenerateTimeBasedGuid(),_reservationId)
            {
                UserId = _userId,
                SeatNumber = _seatNumber
            };
        }
    }
}