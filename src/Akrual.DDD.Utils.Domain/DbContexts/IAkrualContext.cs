using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.DbContexts
{
    public interface IAkrualContext
    {
        void AddOrUpdate<TEntity>(TEntity entity) where TEntity : class, IAggregateRoot;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot;
        IQueryable<TEntity> All<TEntity>(bool forceCaching = false) where TEntity : class, IAggregateRoot;
        void Attach<T>(T op) where T : class, IAggregateRoot;
        void Detach<TEntry>(TEntry entry) where TEntry : class, IAggregateRoot;
        IQueryable<TEntity> FindBy<TEntity>(Expression<Func<TEntity, bool>> selector) where TEntity : class, IAggregateRoot;
        Task<TEntry> GetByIdAsync<TEntry>(Guid id, bool forceCaching = false, bool mustReturnFromDB = false) where TEntry : class, IAggregateRoot;
        void Remove<TEntity>(TEntity entity) where TEntity : class, IAggregateRoot;
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot;
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));
        void UpdateManyToMany<TEntry, TKey>(IEnumerable<TEntry> currentItems, IEnumerable<TEntry> newItems, Func<TEntry, TKey> getKey) where TEntry : class, IAggregateRoot;
        void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot;
    }
}