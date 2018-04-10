using System.Collections.Generic;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>
    /// Domain Command that represents the will for something to happen
    /// </summary>
    public interface IDomainCommand : IHandledMessage<IEnumerable<IMessaging>>
    {
    }
}
