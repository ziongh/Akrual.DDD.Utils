using System;
using System.Collections.Concurrent;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.UOW
{
    /// <summary>
    /// Unit of Work. It will hold all the instances that will be used through out the entire
    /// Use case. And will call the Event store to store the events when it is disposed.
    /// </summary>
    public interface IUnitOfWork
    {
        ConcurrentDictionary<Type,ConcurrentDictionary<Guid, IAggregateRoot>> LoadedAggregates { get; set; }
        IAggregateRoot AddLoadedAggregate(IAggregateRoot entry);
        IAggregateRoot GetLoadedAggregate(Type type, Guid guid);
    }
}