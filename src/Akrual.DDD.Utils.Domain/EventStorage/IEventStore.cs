using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Internal.ConcurrentLists;

namespace Akrual.DDD.Utils.Domain.EventStorage
{
    public interface IEventStore
    {
        Task<IEventStream> GetEventStream<T>(T obj) where T : IAggregateRoot;
        Task SaveNewEvents(Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>> allEventsFromAggregates);
    }
}
