using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>
    /// Represents the interface of a command dispatcher. All Commands will be dispatched through a dispatcher
    /// </summary>
    public interface ICommandDispatcher<in TCommand> : IRequestHandler<TCommand, IEnumerable<IDomainEvent>> where TCommand : IDomainCommand<IEnumerable<IDomainEvent>>
    {
    }
}