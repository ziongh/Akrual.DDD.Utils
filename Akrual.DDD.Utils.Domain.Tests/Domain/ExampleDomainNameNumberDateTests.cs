using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.Coordinator;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.UOW;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class ExampleDomainNameNumberDateTests : TestsUsingSimpleInjector
    {
        [Fact]
        public async Task DispatchCreateCommand_ANewAggregateShouldBeInsertedOnUOW()
        {
            var coordinator = this._container.GetInstance<ICoordinator>();
            var id = GuidGenerator.GenerateTimeBasedGuid();

            var command = new CreateExampleAggregate(id)
            {
                Name = "Name",
                Number = 100,
                Date = new DateTime(2000,01,01)
            };

            await coordinator.DispatchAndApplyEvents(command,CancellationToken.None);

            var uow = this._container.GetInstance<IUnitOfWork>();
            var aggr = uow.LoadedAggregates.First().Value.First().Value;
            var exampleAggre = aggr as ExampleAggregate;
            Assert.Equal(id, aggr.Id);
            Assert.Equal("Name", exampleAggre.Name);
            Assert.Equal(100, exampleAggre.Number);
        }
    }
}
