using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Data.DbContexts;
using Akrual.DDD.Utils.Data.Repositories;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Repositories;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Xunit;

namespace Akrual.DDD.Utils.Data.Tests.Repositories
{
    public class SqlRepositoryTests
    {
        [Fact()]
        public async Task CreateAsOf_GuidThatDoesntExists_ReturnNewAggregate()
        {
            var context = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var repo = new SqlRepository<ExampleAggregate>(new ExampleFactory(),context);
            
            var aggr = await repo.CreateAsOf(Guid.NewGuid());

            Assert.NotNull(aggr);
            Assert.Equal(0, aggr.GetTotalEventsLoaded);
        }

        [Fact()]
        public async Task CreateAsOf_GuidThatExists_ReturnExistingAggregate()
        {
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);

            await aggr.ApplyEvents(new List<IDomainEvent> { new ExampleAggregateCreated(Guid.NewGuid(), aggregateId) });
            
            var context = new FakeAppContext(typeof(ExampleAggregate).Assembly);
            context.AddOrUpdate(aggr);

            var repo = new SqlRepository<ExampleAggregate>(new ExampleFactory(),context);

            // Create Aggregate with same Id
            var fetchedAggr = await repo.CreateAsOf(aggregateId);

            // Should return the original Aggregate
            Assert.NotNull(fetchedAggr);
            Assert.Equal(1, fetchedAggr.GetTotalEventsLoaded);
        }
    }

    internal class ExampleFactory : FactoryBase<ExampleAggregate>
    {
        protected override async Task<ExampleAggregate> CreateDefaultInstance(Guid guid)
        {
            var instantiator = new SimpleInstantiator<ExampleAggregate>();
            return instantiator.Create(guid);
        }
    }
}
