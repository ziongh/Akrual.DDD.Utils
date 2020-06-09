using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.Buses;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.Ledger
{

    public class DepositMoneyBackward : DomainCommand
    {
        public DateTime AppliesAt { get; set; }
        public double Value { get; set; }
        public DepositMoneyBackward(Guid aggregateRootId, Guid sagaId) : base(aggregateRootId, sagaId)
        {
        }

        public DepositMoneyBackward(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return Value;
            yield return AppliesAt;
        }
    }
    public class DepositMoney : DomainCommand
    {
        public double Value { get; set; }
        public DepositMoney(Guid aggregateRootId, Guid sagaId) : base(aggregateRootId, sagaId)
        {
        }

        public DepositMoney(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return Value;
        }
    }

    public class MoneyDeposited : DomainEvent
    {
        public double Value { get; set; }
        public MoneyDeposited(Guid eventGuid, Guid aggregateRootId) : base(eventGuid, aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return Value;
            yield return AppliesAt;
        }

        public override string EventName => "MoneyDeposited";
    }

    public class Account : AggregateRoot<Account>,
    IHandleDomainCommand<DepositMoney>,
    IHandleDomainCommand<DepositMoneyBackward>,
    IHandleDomainEvent<MoneyDeposited>
    {
        private double Balance;
        public override string StreamBaseName => "Account";
        public Account(IBus bus) : base(Guid.Empty,bus)
        {
            Balance = 0;
        }

        public double GetCurrentBalance()
        {
            return Balance;
        }

        public async Task<IEnumerable<IMessaging>> Handle(DepositMoney request, CancellationToken cancellationToken)
        {
            return HandleCommand(request);
        }

        private IEnumerable<IMessaging> HandleCommand(DepositMoney request)
        {
            yield return new MoneyDeposited(GuidGenerator.GenerateTimeBasedGuid(), request.AggregateRootId)
            {
                Value = request.Value,
            };
        }

        public async Task<IEnumerable<IMessaging>> Handle(MoneyDeposited request, CancellationToken cancellationToken)
        {
            Balance += request.Value;
            return null;
        }


        public async Task<IEnumerable<IMessaging>> Handle(DepositMoneyBackward request, CancellationToken cancellationToken)
        {
            return HandleCommandBackwards(request);
        }

        private IEnumerable<IMessaging> HandleCommandBackwards(DepositMoneyBackward request)
        {
            yield return new MoneyDeposited(GuidGenerator.GenerateTimeBasedGuid(),request.AggregateRootId)
            {
                AppliesAt = request.AppliesAt,
                Value = request.Value,
            };
        }
    }
}
