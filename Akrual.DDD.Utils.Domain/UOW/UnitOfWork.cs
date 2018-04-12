using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.UOW
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly EventStore _eventStore;

        public ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>> LoadedAggregates { get; set; }
        public UnitOfWork(EventStore eventStore)
        {
            _eventStore = eventStore;
            LoadedAggregates = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>>();
        }

        public IAggregateRoot AddLoadedAggregate(IAggregateRoot entry)
        {
            var type = (Type) ((dynamic) entry).GetType();
            IAggregateRoot existingEntry = null;
            
            var newdict1 = new ConcurrentDictionary<Guid, IAggregateRoot>();
            newdict1.AddOrUpdate(entry.Id, entry,(guid, root) => root);

            LoadedAggregates.AddOrUpdate(type, newdict1, (type1, roots) =>
            {
                existingEntry = roots.GetOrAdd(entry.Id, entry);
                return roots;
            });

            return existingEntry ?? entry;
        }

        public IAggregateRoot GetLoadedAggregate(Type type, Guid guid)
        {
            if(LoadedAggregates.TryGetValue(type, out var dictOfAggregates))
            {
                if (dictOfAggregates.TryGetValue(guid, out var aggr))
                {
                    return aggr;
                }
            }

            return null;
        }

        public void Dispose()
        {
            var allChanges = new List<IDomainEvent>();

            // Get all Changes from all Aggregates.
            foreach (var listOfAggregateOfSameType in LoadedAggregates)
            {
                foreach (var aggregate in listOfAggregateOfSameType.Value)
                {
                    allChanges.AddRange(aggregate.Value.GetChangesEventStream());
                }
            }

            // Save Events to EventStore.
            _eventStore.SaveNewEvents(allChanges).Wait();

            // Notify Aggregates that all events where Stored.
            foreach (var listOfAggregateOfSameType in LoadedAggregates)
            {
                foreach (var aggregate in listOfAggregateOfSameType.Value)
                {
                    aggregate.Value.AllEventsStored();
                }
            }
        }
    }
}
