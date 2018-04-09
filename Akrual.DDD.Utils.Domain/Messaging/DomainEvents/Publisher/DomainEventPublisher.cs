using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
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
            var handlersFactory = _container.GetInstance<Instantiator<IHandleDomainEvent<Tevent>>>();

            var uow = _container.GetInstance<IUnitOfWork>();

            var factoriesOfHandler = new List<DefaultFactory<IHandleDomainEvent<Tevent>>>();
            var localIndex = 0;

            foreach (var handler in handlersFactory.CreateAllInstances())
            {
                factoriesOfHandler.Add(new DefaultFactory<IHandleDomainEvent<Tevent>>(uow, new StubbedInstantiator<IHandleDomainEvent<Tevent>>(() => GetEventHandler(handlersFactory, localIndex))));
                localIndex++;
            }

            foreach (var factoryOfHandler in factoriesOfHandler)
            {
                var handler = await factoryOfHandler.Create(request.AggregateRootId);
                await handler.ApplyEvents(request);
            }
        }


        public IHandleDomainEvent<Tevent> GetEventHandler<Tevent>(Instantiator<IHandleDomainEvent<Tevent>> handlerFactory, int index)
            where Tevent : IDomainEvent
        {
            var allHandlers = handlerFactory.CreateAllInstances();
            var localIndex = 0;

            foreach (var handler in allHandlers)
            {
                if (localIndex == index) return handler;
                localIndex++;
            }

            return null;
        }
    }
}