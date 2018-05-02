using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate
{
    internal class FactoryBaseWithDefaultObjectCreation : FactoryBase<ExampleAggregate>
    {
        protected override async Task<ExampleAggregate> CreateDefaultInstance(Guid guid)
        {
            var entity = new ExampleAggregate();
            var events = await entity.Handle(new CreateExampleAggregate(guid)
            {
                Name = "OneName",
                Number = 100,
                Date = new DateTime(1990, 5, 12)
            }, CancellationToken.None);

            await entity.ApplyEvents(events.Cast<IDomainEvent>());

            return entity;
        }
    }
}