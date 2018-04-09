using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.UOW;
using SimpleInjector;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents.Publisher
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly Container _container;
        public DomainEventPublisher(Container container)
        {
            _container = container;
        }

        public async Task Publish<Tevent>(Tevent request, CancellationToken cancellationToken) where Tevent : IDomainEvent
        {
            var handlers = _container.GetAllInstances<IHandleDomainEvent<Tevent>>();
            var uow = _container.GetInstance<IUnitOfWork>();

            var factoriesOfHandler = new List<DefaultFactory<IHandleDomainEvent<Tevent>>>();

            for (int i = 0; i < handlers.Count(); i++)
            {
                Func<IHandleDomainEvent<Tevent>> instanceFactory = () =>
                {
                    return GetEventHandler(handlers, i);
                };
                factoriesOfHandler.Add(new DefaultFactory<IHandleDomainEvent<Tevent>>(uow, instanceFactory));
            }

            foreach (var factoryOfHandler in factoriesOfHandler)
            {
                var handler = await factoryOfHandler.Create(request.AggregateRootId);
                await handler.ApplyEvents(request);
            }
        }


        public IHandleDomainEvent<Tevent> GetEventHandler<Tevent>(IEnumerable<IHandleDomainEvent<Tevent>> list, int index)
            where Tevent : IDomainEvent
        {
            var localIndex = 0;
            using (var enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (localIndex == index) return enumerator.Current;
                    localIndex++;
                }
            }

            return null;
        }
    }
}