﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Cache;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.Buses;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.UOW;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Factories
{
    public class DefaultFactoryTests
    {
        [Fact]
        public async Task Create_FactoryWithDefaultFactorySettingName_NameShouldBeSet()
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<ExampleAggregate>();

            var factory = new DefaultFactory<ExampleAggregate>(new UnitOfWork(new InMemoryEventStore(), new MockReadModelCache()),new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName("OneName");
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            Assert.Equal("OneName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

        
        [Fact]
        public async Task Create_TwoTimesWithSameGuid_ReturnsSameInstance()
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<ExampleAggregate>();
            var uow = new UnitOfWork(new InMemoryEventStore(), new MockReadModelCache());
            var factory1 = new DefaultFactory<ExampleAggregate>(uow,new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));
            var factory2 = new DefaultFactory<ExampleAggregate>(uow,new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));

            var id = GuidGenerator.GenerateTimeBasedGuid();

            var exampleAggregate1 = await factory1.Create(id);
            var exampleAggregate2 = await factory2.Create(id);


            Assert.Same(exampleAggregate1, exampleAggregate2);
            Assert.Equal(id, exampleAggregate1.Id);
            Assert.Equal(id, exampleAggregate2.Id);
        }
        
        [Fact]
        public async Task Create_TwoTimesWithSameGuidThenChangeData_ReturnsFirstInstance()
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<ExampleAggregate>();
            var uow = new UnitOfWork(new InMemoryEventStore(), new MockReadModelCache());
            var factory1 = new DefaultFactory<ExampleAggregate>(uow,new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));
            var factory2 = new DefaultFactory<ExampleAggregate>(uow,new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));

            var id = GuidGenerator.GenerateTimeBasedGuid();
            var eventid = GuidGenerator.GenerateTimeBasedGuid();

            var exampleAggregate1 = await factory1.Create(id);

            await exampleAggregate1.Handle(new ExampleAggregateCreated(eventid,id)
            {
                Name = "This Entity Handled one Event",
                Date = DateTime.Now
            }, CancellationToken.None);

            var exampleAggregate2 = await factory2.Create(id);


            Assert.Same(exampleAggregate1, exampleAggregate2);
            Assert.Equal(id, exampleAggregate1.Id);
            Assert.Equal(id, exampleAggregate2.Id);
            Assert.Equal("This Entity Handled one Event", exampleAggregate2.Name);
        }

        [Fact]
        public async Task Create_CreateTwoInstancesChangeOneThenRetrieveTheFirst_ReturnsCorrectName()
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<ExampleAggregate>();
            var uow = new UnitOfWork(new InMemoryEventStore(), new MockReadModelCache());
            var factory1 = new DefaultFactory<ExampleAggregate>(uow,new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));
            var factory2 = new DefaultFactory<ExampleAggregate>(uow,new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));

            var id1 = GuidGenerator.GenerateTimeBasedGuid();
            var id2 = GuidGenerator.GenerateTimeBasedGuid();
            var eventid = GuidGenerator.GenerateTimeBasedGuid();

            var exampleAggregate1 = await factory1.Create(id1);
            var exampleAggregate2 = await factory1.Create(id2);

            await exampleAggregate1.Handle(new ExampleAggregateCreated(eventid,id1)
            {
                Name = "This Entity Handled one Event",
                Date = DateTime.Now
            }, CancellationToken.None);

            var exampleAggregate3 = await factory2.Create(id1);

            Assert.Same(exampleAggregate1, exampleAggregate3);
            Assert.NotSame(exampleAggregate1, exampleAggregate2);
            Assert.Equal("This Entity Handled one Event", exampleAggregate3.Name);
        }

        [Fact]
        public async Task Create_CallTwoTimesFactoryCreate_ReturnsTwoDifferentInstances()
        {
            var bus = new InMemoryBus();
            bus.RegisterHandler<ExampleAggregate>();
            var uow = new UnitOfWork(new InMemoryEventStore(), new MockReadModelCache());
            var factory1 = new DefaultFactory<ExampleAggregate>(uow,new StubbedInstantiator<ExampleAggregate>(() => new ExampleAggregate(bus)));

            var id1 = GuidGenerator.GenerateTimeBasedGuid();
            var id2 = GuidGenerator.GenerateTimeBasedGuid();

            var exampleAggregate1 = await factory1.Create(id1);
            var exampleAggregate2 = await factory1.Create(id2);
            
            Assert.NotSame(exampleAggregate1, exampleAggregate2);
        }

    }


}
