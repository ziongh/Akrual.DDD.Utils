using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Domain.DomainEvents;

namespace Akrual.DDD.Utils.Domain.DomainCommands
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
        IEnumerable<DomainEvent> Handle(ICommandContext context, TCommand command);
    }
}
