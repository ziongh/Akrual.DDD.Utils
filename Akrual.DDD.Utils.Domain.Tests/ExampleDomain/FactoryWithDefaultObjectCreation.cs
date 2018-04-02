using System;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Utils.UUID;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomain
{
    internal class FactoryWithDefaultObjectCreation : Factory<ExampleAggregate, ExampleAggregate>
    {
        public override async Task<ExampleAggregate> CreateDefaultInstance(Guid guid)
        {
            var entity = new ExampleAggregate();
            var events = await entity.Handle(new CreateExampleAggregate(guid)
            {
                Name = "OneName",
                Number = 100,
                Date = new DateTime(1990, 5, 12)
            }, CancellationToken.None);
            
            foreach (var @event in events)
            {
                await entity.Handle((dynamic) @event, CancellationToken.None);
            }

            return entity;
        }
    }
}