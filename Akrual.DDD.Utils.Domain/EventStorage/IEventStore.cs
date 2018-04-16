using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.EventStorage
{
    public interface IEventStore
    {
        Task<IEventStream> GetEventStream<T>(T obj);
        Task SaveNewEvents(IEnumerable<IDomainEvent> events);
    }
}
