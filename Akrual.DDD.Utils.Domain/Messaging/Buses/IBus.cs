using System.Threading;
using System.Threading.Tasks;
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

    public class Bus : IBus
    {
        private readonly Container _container;

        public Bus(Container container)
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
}
