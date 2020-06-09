using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Domain.Tests.Utils;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher;
using SimpleInjector;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class CommandEventTestsWithSimpleInjector : BaseAggregateRootTests<TabAggregate, TabAggregate>
    {
        [Fact]
        public async Task CanOpenANewTab()
        {
            var dispatcher = this._container.GetInstance<IDomainCommandDispatcher>();

            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";
            
            var response = await dispatcher.Dispatch(new OpenTab(testId)
            {
                TableNumber = testTable,
                Waiter = testWaiter
            },CancellationToken.None);
            
            Assert.Single(response);
            Assert.Equal(testId, response.First().AggregateRootId);
            Assert.IsType<TabOpened>(response.First());
        }

        public override void RegisterAllToContainer()
        {
            this._container = new Container();
            ContainerRegistrator.RegisterAllToContainer(_container);
        }
    }
}
