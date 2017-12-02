using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>Represents generic command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface IHandleDomainCommand<in TCommand> where TCommand : IDomainCommand
    {
        /// <summary>Handle the given aggregate command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        IEnumerable<IDomainEvent> Handle(ICommandContext context, TCommand command);
    }
}
