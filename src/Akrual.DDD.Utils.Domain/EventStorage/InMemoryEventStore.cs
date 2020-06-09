using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents.SpecialEvents;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Domain.EventStorage
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, IRecordedEvent>> _database;
        public InMemoryEventStore()
        {
            _database = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, IRecordedEvent>>();
        }

        public async Task<IEventStream> GetEventStream<T>(T obj) where T : IAggregateRoot
        {
            var type = typeof(T).FullName;
            var id = obj.Id;
            var streamName = obj.StreamBaseName + "#" + id.ToString("D");

            if(_database.TryGetValue(streamName, out var dictOfEvents))
            {
                var events = dictOfEvents.Values.AsEnumerable();
                var stream = new EventStream
                {
                    StreamName = streamName,
                    Events = Task.FromResult(events)
                };
                return stream;
            }

            return null;
        }

        public async Task SaveNewEvents(Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>> allEventsFromAggregates)
        {
            foreach (var eventsFromAggregate in allEventsFromAggregates)
            {
                var type = eventsFromAggregate.Key.AggregateType.FullName;
                var id = eventsFromAggregate.Key.AggregateGuid;
                var streamName = eventsFromAggregate.Key.StreamBaseName + "#" + id.ToString("D");

                IRecordedEvent existingEntry = null;

                foreach (var @event in eventsFromAggregate.Value)
                {
                    var newdict1 = new ConcurrentDictionary<Guid, IRecordedEvent>();
                    var newRecordedEvent = new RecordedEvent()
                    {
                        Event = @event,
                        CreatedAt = DateTimeProvider.Current.UtcNow
                    };

                    newdict1.TryAdd(@event.EventGuid, newRecordedEvent);

                    _database.AddOrUpdate(streamName, newdict1, (_streamName, events) =>
                    {
                        existingEntry = events.GetOrAdd(@event.EventGuid, newRecordedEvent);
                        return events;
                    });
                }
            }
        }
    }
}