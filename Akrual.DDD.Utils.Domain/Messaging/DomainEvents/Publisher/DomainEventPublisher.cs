using System;
using System.Collections.Generic;
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
            var handlers = _container.GetAllInstances<Func<IHandleDomainEvent<Tevent>>>();
            var uow = _container.GetInstance<IUnitOfWork>();

            var factoriesOfHandler = new List<DefaultEventHandlerFactory<IHandleDomainEvent<Tevent>>>();
            foreach (var handler in handlers)
            {
                factoriesOfHandler.Add(new DefaultEventHandlerFactory<IHandleDomainEvent<Tevent>>(uow, handler));
            }


            foreach (var factoryOfHandler in factoriesOfHandler)
            {
                var handler = await factoryOfHandler.Create(request.AggregateRootId);
                await handler.ApplyEvents(request);
            }
        }
    }
}