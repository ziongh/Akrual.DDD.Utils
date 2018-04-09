using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.Saga;
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

        public async Task<IEnumerable<DomainCommand>> Publish<Tevent>(Tevent request, CancellationToken cancellationToken) where Tevent : IDomainEvent
        {
            var handlersFactory = _container.GetInstance<Instantiator<IHandleDomainEvent<Tevent>>>();
            IProcessManagerRedirect<Tevent> sagaCoord = null;

            try
            {
                sagaCoord = _container.GetInstance<IProcessManagerRedirect<Tevent>>();
            }
            catch (Exception e)
            {
            }

            var uow = _container.GetInstance<IUnitOfWork>();

            var factoriesOfHandler = new List<DefaultFactory<IHandleDomainEvent<Tevent>>>();
            var localIndex = 0;

            foreach (var handler in handlersFactory.CreateAllInstances())
            {
                var index = localIndex;
                factoriesOfHandler.Add(new DefaultFactory<IHandleDomainEvent<Tevent>>(uow, new StubbedInstantiator<IHandleDomainEvent<Tevent>>(() =>
                {
                    var temp = index;
                    return GetEventHandler(handlersFactory, temp);
                })));
                localIndex++;
            }

            foreach (var factoryOfHandler in factoriesOfHandler)
            {
                var handler = await factoryOfHandler.Create(request.AggregateRootId);
                await handler.ApplyEvents(request);
            }

            if (sagaCoord != null)
            {
                var messages = sagaCoord.Redirect(request);
                var events = messages.OfType<IDomainEvent>();
                var commands = messages.OfType<DomainCommand>();
                foreach (var @event in events)
                {
                    await Publish((dynamic) @event, cancellationToken);
                }

                return commands;
            }

            return new DomainCommand[0];
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