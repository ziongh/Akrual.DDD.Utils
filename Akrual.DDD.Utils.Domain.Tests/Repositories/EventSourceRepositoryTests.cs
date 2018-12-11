using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Repositories;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.Ledger;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Repositories
{
    public class EventSourceRepositoryTests
    {
        [Fact]
        public async Task CreateAsOf_GuidThatDoesntExists_ReturnNewAggregate()
        {
            var eventStore = new InMemoryEventStore();
            var repository = new EventSourceRepository<ExampleAggregate>(new ExampleFactory(), eventStore);

            var aggr = await repository.CreateAsOf(Guid.NewGuid());

            Assert.NotNull(aggr);
            Assert.Equal(0, aggr.GetTotalEventsLoaded);
        }

        [Fact]
        public async Task CreateAsOf_GuidThatExists_ReturnExistingAggregate()
        {
            // Create Event Store and Populate with one entry
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);

            var eventstore = new InMemoryEventStore();
            var allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();
            allChanges.Add(new EventStreamNameComponents(typeof(ExampleAggregate), aggr.Id), new List<IDomainEvent> { new ExampleAggregateCreated(Guid.NewGuid(), aggregateId) });
            await eventstore.SaveNewEvents(allChanges);
            var repository = new EventSourceRepository<ExampleAggregate>(new ExampleFactory(), eventstore);

            // Create Aggregate with same Id
            var fetchedAggr = await repository.CreateAsOf(aggregateId);

            // Should return the original Aggregate
            Assert.NotNull(fetchedAggr);
            Assert.Equal(1, fetchedAggr.GetTotalEventsLoadedFromDB);
            Assert.Equal(1, fetchedAggr.GetTotalEventsLoaded);
        }


        [Fact]
        public async Task CreateAsOf_AccountWithRetroactiveEvents_ReturnCorrectBalance()
        {
            // Create Event Store and Populate with one entry
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var eventstore = await CreateLedger(aggregateId);

            var repository = new EventSourceRepository<Account>(new ExampleFactoryAccount(), eventstore);

            // Create Aggregate with same Id
            var fetchedAggr = await repository.CreateAsOf(aggregateId, new DateTime(2010, 01, 04,23,01,01));

            // Should return the original Aggregate
            Assert.Equal(3800, fetchedAggr.GetCurrentBalance());
        }

        [Fact]
        public async Task CreateAsAt_AccountWithRetroactiveEvents_ReturnCorrectBalance()
        {
            // Create Event Store and Populate with one entry
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var eventstore = await CreateLedger(aggregateId);

            var repository = new EventSourceRepository<Account>(new ExampleFactoryAccount(), eventstore);

            // Create Aggregate with same Id
            var fetchedAggr = await repository.CreateAsAt(aggregateId, new DateTime(2010, 01, 04,23,01,01));

            // Should return the original Aggregate
            Assert.Equal(1800, fetchedAggr.GetCurrentBalance());
        }


        private static async Task<InMemoryEventStore> CreateLedger(Guid aggregateId)
        {
            var aggr = new SimpleInstantiator<Account>().Create(aggregateId);

            var eventstore = new InMemoryEventStore();

            var allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();
            allChanges.Add(new EventStreamNameComponents(typeof(Account), aggr.Id),
                new List<IDomainEvent> {new MoneyDeposited(Guid.NewGuid(), aggregateId) {Value = 1500}});
            DateTimeProvider.Current = new FakeDateTimeProvider(new DateTime(2010, 01, 01));
            await eventstore.SaveNewEvents(allChanges);

            allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();
            allChanges.Add(new EventStreamNameComponents(typeof(Account), aggr.Id),
                new List<IDomainEvent> {new MoneyDeposited(Guid.NewGuid(), aggregateId) {Value = 300}});
            DateTimeProvider.Current = new FakeDateTimeProvider(new DateTime(2010, 01, 03));
            await eventstore.SaveNewEvents(allChanges);

            allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();
            allChanges.Add(new EventStreamNameComponents(typeof(Account), aggr.Id),
                new List<IDomainEvent> {new MoneyDeposited(Guid.NewGuid(), aggregateId) {Value = 200}});
            DateTimeProvider.Current = new FakeDateTimeProvider(new DateTime(2010, 01, 05));
            await eventstore.SaveNewEvents(allChanges);

            allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();
            allChanges.Add(new EventStreamNameComponents(typeof(Account), aggr.Id),
                new List<IDomainEvent>
                {
                    new MoneyDeposited(Guid.NewGuid(), aggregateId) {Value = 2000, AppliesAt = new DateTime(2010, 01, 04)}
                });
            DateTimeProvider.Current = new FakeDateTimeProvider(new DateTime(2010, 01, 07));
            await eventstore.SaveNewEvents(allChanges);
            return eventstore;
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
    internal class ExampleFactoryAccount : FactoryBase<Account>
    {
        protected override async Task<Account> CreateDefaultInstance(Guid guid)
        {
            var instantiator = new SimpleInstantiator<Account>();
            return instantiator.Create(guid);
        }
    }
}
