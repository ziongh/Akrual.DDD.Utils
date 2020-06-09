using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Cache;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation.Aggregates;
using Akrual.DDD.Utils.Domain.UOW;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using SimpleInjector;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.EventStorage
{
    public class EventStoreTests
    {
        [Fact]
        public async Task AddToStream_NewEvent_CorrectStreamName()
        {
            var aggregateId = GuidGenerator.GenerateTimeBasedGuid();
            var aggr = new SimpleInstantiator<ExampleAggregate>().Create(aggregateId);

            var eventstore = new InMemoryEventStore();
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

            var eventstore = new InMemoryEventStore();
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

            var eventstore = new InMemoryEventStore();
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

            var eventstore = new InMemoryEventStore();
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

            var eventstore = new InMemoryEventStore();
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
}
