using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Domain.Tests.Utils;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.Buses;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using MessagePack;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class CommandEventTests : BaseAggregateRootTests<TabAggregate, TabAggregate>
    {
        [Fact]
        public async Task CanOpenANewTab()
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<TabAggregate>();
            var testId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            await Test(new TabAggregate(bus), 
                Given(),
                When(new OpenTab(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                Then(new TabOpened(eventId,testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }));
        }

        [Fact]
        public async Task CannotOpenTabTwice()
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<TabAggregate>();
            var testId = GuidGenerator.GenerateTimeBasedGuid();
            var eventId = GuidGenerator.GenerateTimeBasedGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            await Test(new TabAggregate(bus), 
                Given(new TabOpened(eventId, testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                When(new OpenTab(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                ThenFailWith<TabOpenedTwiceException>());
        }

        public override void RegisterAllToContainer()
        {
        }
    }

    [MessagePackObject]
    public class TabOpened : DomainEvent
    {
        [Key(5)]
        public override string EventName { get; } = "TabOpened";


        [Key(8)]
        public int TableNumber;
        [Key(9)]
        public string Waiter;

        public TabOpened(Guid eventId, Guid aggregateRootId) : base(eventId, aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return TableNumber;
            yield return Waiter;
        }

    }

    public class OpenTab : DomainCommand
    {
        public int TableNumber;
        public string Waiter;

        public OpenTab(Guid aggregateRootId, Guid sagaId) : base(aggregateRootId, sagaId)
        {
        }

        public OpenTab(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return TableNumber;
            yield return Waiter;
        }
    }

    public class TabOpenedTwiceException : DomainException
    {
        public Guid Service_Id {get;set;}
    }


    [MessagePackObject]
    public class TabAggregate : 
        AggregateRoot<TabAggregate>, 
        IHandleDomainCommand<OpenTab>,
        IHandleDomainEvent<TabOpened>
    {
        [Key(6)]
        public int TableNumber { get; private set; }
        [Key(7)]
        public string Waiter { get; private set; }
        [Key(8)]
        public bool Opened { get; set; }

        [Key(5)]
        public override string StreamBaseName => "Tab";

        public TabAggregate(IBus bus) : base(Guid.Empty, bus)
        {
        }

        public async Task<IEnumerable<IMessaging>> Handle(OpenTab command, CancellationToken cancellationToken)
        {
            if (Opened)
                throw new TabOpenedTwiceException();

            return GetEvents(command);
        }

        private IEnumerable<IDomainEvent> GetEvents(OpenTab command)
        {
            yield return new TabOpened(Guid.NewGuid(),command.AggregateRootId){TableNumber = command.TableNumber, Waiter = command.Waiter};
        }

        public async Task<IEnumerable<IMessaging>> Handle(TabOpened notification, CancellationToken cancellationToken)
        {
            Opened = true;

            return new List<IMessaging>();
        }
    }
}
