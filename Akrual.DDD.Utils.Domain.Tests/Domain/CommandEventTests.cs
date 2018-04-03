using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Domain.Tests.Utils;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Entities;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using MediatR;
using MediatR.Pipeline;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class CommandEventTests : BaseAggregateRootTests<TabAggregate, TabAggregate>
    {
        [Fact]
        public async Task CanOpenANewTab()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            await Test(new TabAggregate(), 
                Given(),
                When(new OpenTab(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                Then(new TabOpened(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }));
        }

        [Fact]
        public async Task CannotOpenTabTwice()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            await Test(new TabAggregate(), 
                Given(new TabOpened(testId)
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

    public class TabOpened : DomainEvent
    {
        public int TableNumber;
        public string Waiter;

        public TabOpened(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public TabOpened(Guid aggregateRootId) : base(aggregateRootId)
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

        public OpenTab(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
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
        
    }


    public class TabAggregate : 
        AggregateRoot<TabAggregate>, 
        IHandleDomainCommand<OpenTab>,
        IHandleDomainEvent<TabOpened>
    {
        public int TableNumber { get; private set; }
        public string Waiter { get; private set; }
        public bool Opened { get; set; }

        public TabAggregate() : base(Guid.Empty)
        {
        }

        public async Task<IEnumerable<IDomainEvent>> Handle(OpenTab command, CancellationToken cancellationToken)
        {
            if (Opened)
                throw new TabOpenedTwiceException();

            return GetEvents(command);
        }

        private IEnumerable<IDomainEvent> GetEvents(OpenTab command)
        {
            yield return new TabOpened(command.AggregateRootId){TableNumber = command.TableNumber, Waiter = command.Waiter};
        }

        public async Task Handle(TabOpened notification, CancellationToken cancellationToken)
        {
            Opened = true;
        }
    }
}
