using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher
{
    /// <summary>
    /// Represents the interface of a command dispatcher. All Commands will be dispatched through a dispatcher
    /// </summary>
    public interface IDomainCommandDispatcher
    {
        Task<IEnumerable<IDomainEvent>> Dispatch<Tcommand>(Tcommand request,
            CancellationToken cancellationToken) where Tcommand : IDomainCommand<IEnumerable<IDomainEvent>>;
    }
}