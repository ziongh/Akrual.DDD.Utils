using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Data.Repositories.DbContexts;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Repositories;
using Akrual.DDD.Utils.Domain.Repositories.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Akrual.DDD.Utils.Data.Repositories
{
    /// <summary>
    /// Base class for implementing asynchronous domain repositories for aggregate root types.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
    /// <typeparam name="T">The type of the aggregate root Inner object.</typeparam>
    public abstract class AsyncDomainRepositoryBase<TAggregate,T> : IAsyncDomainRepository<TAggregate,T>
        where TAggregate : AggregateRoot<T>
        where T : new()
    {
        protected readonly DBContextOfAggregate _dbContext;
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainRepositoryBase{TAggregate}"/> class.
        /// </summary>
        protected AsyncDomainRepositoryBase(DBContextOfAggregate dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Asynchronously finds an aggregate root instance from the repository using its id.
        /// </summary>
        /// <param name="id">Aggregate root instance id.</param>
        /// <returns>Aggregate root instance.</returns>
        public virtual Task<TAggregate> FindByIdAsync(Guid id)
        {
            return _dbContext.Set<TAggregate>().AsQueryable().FirstOrDefaultAsync(s => s.Id == id);
        }

        public virtual Task<List<TAggregate>> List(ISpecification<TAggregate> spec)
        {
            // fetch a Queryable that includes all expression-based includes
            var queryableResultWithIncludes = spec.Includes
                .Aggregate(_dbContext.Set<TAggregate>().AsQueryable(),
                    (current, include) => current.Include(include));
        
            // return the result of the query using the specification's criteria expression
            return queryableResultWithIncludes
                .Where(spec.Criteria)
                .ToListAsync();
        }
    }
}
