using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Data.EventStore;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Cache;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates;
using Akrual.DDD.Utils.Domain.UOW;
using Akrual.DDD.Utils.Domain.Utils.TypeCache;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Akrual.DDD.Utils.Internal.Serializer;
using MessagePack.Resolvers;
using SimpleInjector;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.EventStorage
{
    public class AzureTableEventStoreTests : IClassFixture<CacheFisxture>
    {
        private readonly CacheFisxture fixture;

        public AzureTableEventStoreTests(CacheFisxture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task AddToStream_NewEvent_CorrectStreamName()
        {
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);

            var eventstore = new AzureTableEventStore(new DomainTypeFinder(typeof(ExampleAggregate).Assembly));
            var allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();
            allChanges.Add(new EventStreamNameComponents(typeof(ExampleAggregate), aggr.Id, aggr.StreamBaseName), new List<IDomainEvent>{new ExampleAggregateCreated(Guid.NewGuid(),aggregateId )});
            await eventstore.SaveNewEvents(allChanges);

            var eventStream = await eventstore.GetEventStream(aggr);

            Assert.Equal(aggr.StreamBaseName + "#" + aggr.Id.ToString("D"), eventStream.StreamName);
        }

        [Fact]
        public async Task AddToStream_ThenGet_NewEvent_ReturnCorrectEvent()
        {
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);
            var @event = new ExampleAggregateCreated(Guid.NewGuid(),aggregateId );

            var eventstore = new AzureTableEventStore(new DomainTypeFinder(typeof(ExampleAggregate).Assembly));
            var allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();
            allChanges.Add(new EventStreamNameComponents(typeof(ExampleAggregate), aggr.Id, aggr.StreamBaseName), new List<IDomainEvent>{@event});
            await eventstore.SaveNewEvents(allChanges);

            var eventStream = await eventstore.GetEventStream(aggr);
            var events = (await eventStream.Events).ToList();

            Assert.Single(events);
            Assert.Equal(@event, events.First().Event);
        }

        [Fact]
        public async Task DisposeUnitOfWork_AfterHavingAddedEvents_CorrectStreamName()
        {
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);
            var @event = new ExampleAggregateCreated(Guid.NewGuid(),aggregateId );

            var eventstore = new AzureTableEventStore(new DomainTypeFinder(typeof(ExampleAggregate).Assembly));
            using (var uow = new UnitOfWork(eventstore, new MockReadModelCache()))
            {
                uow.AddLoadedAggregate(aggr);
                await aggr.ApplyEvents(@event);
            }

            var eventStream = await eventstore.GetEventStream(aggr);

            Assert.Equal(aggr.StreamBaseName + "#" + aggr.Id.ToString("D"), eventStream.StreamName);
        }

        [Fact]
        public async Task DisposeUnitOfWork_AfterHavingAddedTwoEvents_CorrectStreamName()
        {
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggregateId2 = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);
            var aggr2 = new SimpleInstantiator<Order>().Create(aggregateId2);

            var @event = new ExampleAggregateCreated(Guid.NewGuid(),aggregateId );
            var @eventOrder = new _6OrderConfirmed_Order(Guid.NewGuid(),aggregateId2 );

            var eventstore = new AzureTableEventStore(new DomainTypeFinder(typeof(ExampleAggregate).Assembly));
            using (var uow = new UnitOfWork(eventstore, new MockReadModelCache()))
            {
                uow.AddLoadedAggregate(aggr2);
                uow.AddLoadedAggregate(aggr);
                await aggr2.ApplyEvents(@eventOrder);
                await aggr.ApplyEvents(@event);
            }

            var eventStream = await eventstore.GetEventStream(aggr);

            Assert.Equal(aggr.StreamBaseName + "#" + aggr.Id.ToString("D"), eventStream.StreamName);
        }

        [Fact]
        public async Task DisposeUnitOfWork_AfterHavingAddedEvents_ReturnCorrectEvent()
        {
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);
            var @event = new ExampleAggregateCreated(Guid.NewGuid(),aggregateId );

            var eventstore = new AzureTableEventStore(new DomainTypeFinder(typeof(ExampleAggregate).Assembly));
            using (var uow = new UnitOfWork(eventstore, new MockReadModelCache()))
            {
                uow.AddLoadedAggregate(aggr);
                await aggr.ApplyEvents(@event);
            }

            var eventStream = await eventstore.GetEventStream(aggr);
            var events = (await eventStream.Events).ToList();

            Assert.Single(events);
            Assert.Equal(@event, events.First().Event);
        }
    }



    public class CacheFisxture : IDisposable
    {
        public CacheFisxture()
        {
            Environment.SetEnvironmentVariable("client","Development");
            Environment.SetEnvironmentVariable("StorageConnectionString","lala");

            MessagePackSerializerLz4.AddTypesThatHaveUnions(new List<Tuple<Type, IEnumerable<Assembly>>>
            {
                new Tuple<Type, IEnumerable<Assembly>>(typeof(DomainEvent), new List<Assembly>{ typeof(CreateExampleAggregate).Assembly }),
                new Tuple<Type, IEnumerable<Assembly>>(typeof(DomainCommand), new List<Assembly>{ typeof(CreateExampleAggregate).Assembly }),
                new Tuple<Type, IEnumerable<Assembly>>(typeof(AggregateRoot<>), new List<Assembly>{ typeof(ExampleAggregate).Assembly }),
                //new Tuple<Type, IEnumerable<Assembly>>(typeof(IDomainEvent), new List<Assembly>{ typeof(CreateExampleAggregate).Assembly }),
                //new Tuple<Type, IEnumerable<Assembly>>(typeof(IDomainCommand), new List<Assembly>{ typeof(CreateExampleAggregate).Assembly }),
                //new Tuple<Type, IEnumerable<Assembly>>(typeof(IAggregateRoot), new List<Assembly>{ typeof(ExampleAggregate).Assembly }),
            });
        }

        public void Dispose()
        {
            // ... clean up test data from the database ...
        }
    }
}
