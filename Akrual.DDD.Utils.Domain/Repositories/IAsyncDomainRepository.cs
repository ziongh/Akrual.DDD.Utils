using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Repositories.Specifications;

namespace Akrual.DDD.Utils.Domain.Repositories
{
    /// <summary>
    /// Asynchronous domain repository for a given aggregate root type.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
    /// <typeparam name="T">The type of the aggregate root Inner object.</typeparam>
    public interface IAsyncDomainRepository<TAggregate,T> where TAggregate : AggregateRoot<T>
    {
        /// <summary>
        /// Asynchronously finds an aggregate root instance from the repository using its id.
        /// </summary>
        /// <param name="id">Aggregate root instance id.</param>
        /// <returns>Aggregate root instance.</returns>
        Task<TAggregate> FindByIdAsync(Guid id);

        Task<List<TAggregate>> List(ISpecification<TAggregate> spec);
    }
}
