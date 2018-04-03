using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Messaging.Saga
{
    /// <summary>
    /// Interface that must be added to a Process Manager Class for each redirection that it will implement.
    /// </summary>
    /// <typeparam name="TInEvent">The event that the process manager will receive</typeparam>
    public interface IProcessManagerRedirect<in TInEvent> 
        where TInEvent : IDomainEvent
    {
        /// <summary>
        /// For a given Event, it return a list of messages (Events and Commands) that shall be published/dispatched
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IEnumerable<IMessaging> Redirect(TInEvent message);
    }
}