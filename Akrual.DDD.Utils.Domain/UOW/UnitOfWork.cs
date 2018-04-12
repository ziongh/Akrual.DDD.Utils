using System;
using System.Collections.Concurrent;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        public ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>> LoadedAggregates { get; set; }
        public UnitOfWork()
        {
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
    }
}
