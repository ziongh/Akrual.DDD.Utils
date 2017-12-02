using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Domain.Tests.Utils;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class CommandEventTests : BaseAggregateRootTests<TabAggregate, TabAggregate>
    {
        [Fact]
        public void CanOpenANewTab()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            Test(new TabAggregate(testId), 
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
        public void CannotOpenTabTwice()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            Test(new TabAggregate(testId), 
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
        IApplyDomainEvent<TabOpened>
    {
        public int TableNumber { get; private set; }
        public string Waiter { get; private set; }
        public bool Opened { get; set; }

        public TabAggregate(Guid id) : base(id)
        {
            Opened = false;
        }

        public IEnumerable<IDomainEvent> Handle(ICommandContext context, OpenTab command)
        {
            if (Opened)
                throw new TabOpenedTwiceException();
            yield return new TabOpened(command.AggregateRootId){ TableNumber = command.TableNumber, Waiter = command.Waiter};
        }

        public void Apply(TabOpened e)
        {
            Opened = true;
        }
    }
}
