using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using SimpleInjector;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Container _container;
        public CommandDispatcher(Container container)
        {
            _container = container;
        }

        public async Task<IEnumerable<IDomainEvent>> Dispatch<Tcommand>(Tcommand request,
            CancellationToken cancellationToken) where Tcommand : IDomainCommand<IEnumerable<IDomainEvent>>
        {
            var factoryOfHandler = _container.GetInstance<IDefaultFactory<IHandleDomainCommand<Tcommand>>>();

            var handler = await factoryOfHandler.Create(request.AggregateRootId);

            return await handler.Handle(request, cancellationToken);
        }
    }
}