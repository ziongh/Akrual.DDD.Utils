using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>Represents generic command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface IHandleDomainCommand<in TCommand> : IRequestHandler<TCommand, IEnumerable<IDomainEvent>> where TCommand : IDomainCommand<IEnumerable<IDomainEvent>>
    {
    }
}
