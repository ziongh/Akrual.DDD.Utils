using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Repositories;
using SimpleInjector;

namespace Akrual.DDD.Utils.Domain.Messaging.Buses
{
    public interface IBus
    {
        /// <summary>
        /// Represents the interface of a command dispatcher. All Commands will be dispatched through a dispatcher
        /// </summary>
        Task Dispatch<Tcommand>(Tcommand request, CancellationToken cancellationToken) where Tcommand : IDomainCommand;

        /// <summary>
        /// Represents the interface of a Event Publisher. All Events will be published through a publisher
        /// </summary>
        Task Publish<Tevent>(Tevent request, CancellationToken cancellationToken) where Tevent : IDomainEvent;
    }

    public class InProccessDIBus : IBus
    {
        private readonly Container _container;

        public InProccessDIBus(Container container)
        {
            _container = container;
        }

        public async Task Dispatch<Tcommand>(Tcommand request, CancellationToken cancellationToken) where Tcommand : IDomainCommand
        {
            // Get FactoryBase of Command Handler
            var repositoryOfHandler = _container.GetInstance<IRepository<IHandleDomainCommand<Tcommand>>>();

            // Creates the Command Handler (The Aggregate)
            var handler = await repositoryOfHandler.CreateAsOf(request.AggregateRootId);

            // Handle the Command with the Command Handler (Aggregate) and returns all messages
            await handler.Handle(request, cancellationToken);
        }

        public async Task Publish<Tevent>(Tevent request, CancellationToken cancellationToken)
            where Tevent : IDomainEvent
        {
            // Get FactoryBase of Event Handler
            var repositoryOfHandler = _container.GetInstance<IRepository<IHandleDomainEvent<Tevent>>>();

            // Creates the Event Handler (The Aggregate)
            var handler = await repositoryOfHandler.CreateAsOf(request.AggregateRootId);

            // Apply Events into the EventHandler (The Aggregate)
            await handler.ApplyEvents(request);
        }

    }



    public class InMemoryBus : IBus
    {
        private static readonly IList<Type> RegisteredHandlers = new List<Type>();


        #region IBus
        public async Task Dispatch<Tcommand>(Tcommand request, CancellationToken cancellationToken) where Tcommand : IDomainCommand
        {
            var messageType = request.GetType();
            var openInterface = typeof(IHandleDomainCommand<>);
            var closedInterface = openInterface.MakeGenericType(messageType);
            var handlersToNotify = from h in RegisteredHandlers
                where closedInterface.IsAssignableFrom(h)
                select h;
            foreach (var h in handlersToNotify)
            {
                dynamic sagaInstance = Activator.CreateInstance(h, this);     // default ctor is enough
                sagaInstance.Handle(request);
            }
        }

        public async Task Publish<Tevent>(Tevent request, CancellationToken cancellationToken) where Tevent : IDomainEvent
        {
            var messageType = request.GetType();
            var openInterface = typeof(IHandleDomainEvent<>);
            var closedInterface = openInterface.MakeGenericType(messageType);
            var handlersToNotify = from h in RegisteredHandlers
                where closedInterface.IsAssignableFrom(h)
                select h;
            foreach (var h in handlersToNotify)
            {
                dynamic sagaInstance = Activator.CreateInstance(h, this);     // default ctor is enough
                sagaInstance.Handle(request);
            }
        }

        public void RegisterHandler<T>()
        {
            RegisteredHandlers.Add(typeof(T));
        }
        #endregion
      
    }
}
