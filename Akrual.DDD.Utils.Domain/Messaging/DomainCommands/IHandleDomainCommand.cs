using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Aggregates;
using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>Represents generic command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface IHandleDomainCommand<in TCommand> : IAggregateRoot, IRequestHandler<TCommand, IEnumerable<IMessaging>> where TCommand : IDomainCommand
    {
    }
}
