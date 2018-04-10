using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
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

        public async Task<IEnumerable<IDomainCommand>> Publish<Tevent>(Tevent request, CancellationToken cancellationToken) where Tevent : IDomainEvent
        {
            // Get Factory of Event Handler
            var factoryOfHandler = _container.GetInstance<IDefaultFactory<IHandleDomainEvent<Tevent>>>();

            // Creates the Event Handler (The Aggregate)
            var handler = await factoryOfHandler.Create(request.AggregateRootId);

            // Apply Events into the EventHandler (The Aggregate)
            var messages = (await handler.ApplyEvents(request)).ToList();

            // Get All Events that this event handler has emitted
            var events = messages.OfType<IDomainEvent>().ToList();

            // Get All Commands that this event handler has emitted
            var commands = messages.OfType<IDomainCommand>().ToList();

            // Foreach event emitted, publish them recursively
            foreach (var @event in events)
            {
                IEnumerable<IDomainCommand> newCommands = await Publish((dynamic) @event, cancellationToken);
                commands.AddRange(newCommands);
            }

            // Return all Commands that the event handler and all its recursions generated.
            return commands;
        }
    }
}