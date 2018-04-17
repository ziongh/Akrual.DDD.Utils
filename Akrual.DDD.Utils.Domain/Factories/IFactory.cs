using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    /// <summary>
    /// This interface represents a default implementation of an FactoryBase
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFactory<T> where T : IAggregateRoot
    {
        event EventHandler<FactoryCreationExecutingContext<T, T>> OnAfterCreateDefaultInstance;
        
        Task<T> Create(Guid guid);
    }
}