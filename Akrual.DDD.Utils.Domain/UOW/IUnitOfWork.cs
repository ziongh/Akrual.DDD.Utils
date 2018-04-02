using System;
using System.Collections.Concurrent;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.UOW
{
    public interface IUnitOfWork
    {
        ConcurrentDictionary<Type,ConcurrentDictionary<Guid, IAggregateRoot>> LoadedAggregates { get; set; }
        IAggregateRoot AddLoadedAggregate(IAggregateRoot entry);
        IAggregateRoot GetLoadedAggregate(Type type, Guid guid);
    }
}