using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    /// <summary>
    /// This interface represents a default implementation of an Factory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDefaultFactory<T> where T : IAggregateRoot
    {
        Task<T> CreateDefaultInstance(Guid guid);
        event EventHandler<FactoryCreationExecutingContext<T, T>> OnAfterCreateDefaultInstance;

        /// <summary>
        /// Creates the Aggregate with all the invariants Checked
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        Task<T> Create(Guid guid);
    }
    
    /// <summary>
    /// This interface represents a default implementation of an Factory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDefaultEventHandlerFactory<T> where T : IAggregateRoot
    {
        Task<T> CreateDefaultInstance(Guid guid);
        event EventHandler<FactoryCreationExecutingContext<T, T>> OnAfterCreateDefaultInstance;

        /// <summary>
        /// Creates the Aggregate with all the invariants Checked
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        Task<T> Create(Guid guid);
    }
}