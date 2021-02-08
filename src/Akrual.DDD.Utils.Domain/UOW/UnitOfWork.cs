using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Cache;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.UOW
{
    /// <summary>
    /// Unit of Work. It will hold all the instances that will be used through out the entire
    /// Use case. And will call the Event store to store the events when it is disposed.
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IEventStore _eventStore;
        private readonly IReadModelCache _readModelCache;

        public ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>> LoadedAggregates { get; set; }
        public UnitOfWork(IEventStore eventStore, IReadModelCache readmodelCache)
        {
            _eventStore = eventStore;
            this._readModelCache = readmodelCache;
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
            var allChanges = new Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>>();


            // Get all Changes from all Aggregates.
            foreach (var listOfAggregateOfSameType in LoadedAggregates)
            {
                foreach (var aggregate in listOfAggregateOfSameType.Value)
                {
                    allChanges.Add(new EventStreamNameComponents(aggregate.Value.GetType(),aggregate.Value.Id, aggregate.Value.StreamBaseName), aggregate.Value.GetChangesEventStream());

                    if(!_readModelCache.AddAsync(aggregate.Value.StreamBaseName + "_" +aggregate.Value.Id.ToString("D"), aggregate.Value, DateTime.UtcNow.AddDays(7)).Result)
                    {
                        throw new DBConcurrencyException("Tentativa de escrita no Redis falhou, pois alguém alterou antes.");
                    }
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
