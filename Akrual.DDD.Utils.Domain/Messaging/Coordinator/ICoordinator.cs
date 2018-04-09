using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents.Publisher;

namespace Akrual.DDD.Utils.Domain.Messaging.Coordinator
{
    public interface ICoordinator
    {
        Task DispatchAndApplyEvents<Tcommand>(Tcommand request,
            CancellationToken cancellationToken) where Tcommand : IDomainCommand<IEnumerable<IDomainEvent>>;
    }

    public class Coordinator : ICoordinator
    {
        private readonly IDomainCommandDispatcher _commandDispatcher;
        private readonly IDomainEventPublisher _eventPublisher;
        public Coordinator(IDomainCommandDispatcher commandDispatcher, IDomainEventPublisher eventPublisher)
        {
            _commandDispatcher = commandDispatcher;
            _eventPublisher = eventPublisher;
        }

        public async Task DispatchAndApplyEvents<Tcommand>(Tcommand request,
            CancellationToken cancellationToken)
            where Tcommand : IDomainCommand<IEnumerable<IDomainEvent>>
        {
            var events = await _commandDispatcher.Dispatch(request, cancellationToken);

            List<DomainCommand> commands = new List<DomainCommand>();

            foreach (var @event in events)
            {
                commands.AddRange(await _eventPublisher.Publish((dynamic)@event, cancellationToken));
            }

            foreach (var command in commands)
            {
                await DispatchAndApplyEvents((dynamic)command, cancellationToken);
            }
        }
    }
}
