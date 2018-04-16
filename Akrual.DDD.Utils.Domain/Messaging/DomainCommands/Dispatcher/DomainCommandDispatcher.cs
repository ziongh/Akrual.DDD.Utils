using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using SimpleInjector;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher
{
    public class DomainCommandDispatcher : IDomainCommandDispatcher
    {
        private readonly Container _container;

        public DomainCommandDispatcher(Container container)
        {
            _container = container;
        }

        public async Task<IEnumerable<IDomainEvent>> Dispatch<Tcommand>(Tcommand request,
            CancellationToken cancellationToken) where Tcommand : IDomainCommand
        {
            // Get Factory of Command Handler
            var factoryOfHandler = _container.GetInstance<IDefaultFactory<IHandleDomainCommand<Tcommand>>>();

            // Creates the Command Handler (The Aggregate)
            var handler = await factoryOfHandler.CreateAsOf(request.AggregateRootId);

            // Handle the Command with the Command Handler (Aggregate) and returns all messages
            var messages = await handler.Handle(request, cancellationToken);

            // Get All Events that this Command handler has emitted
            var events = messages.OfType<IDomainEvent>().ToList();

            // Get All Commands that this Command handler has emitted
            var commands = messages.OfType<IDomainCommand>().ToList();

            // Foreach Command emitted, Handle them recursively
            foreach (var command in commands)
            {
                IEnumerable<IDomainEvent> newEvents = await Dispatch((dynamic) command, cancellationToken);
                events.AddRange(newEvents);
            }

            // Return all Events that the Command handler and all its recursions generated.
            return events;
        }
    }
}