using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.DbContexts;
using Akrual.DDD.Utils.Domain.Repositories;
using Akrual.DDD.Utils.Internal.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Akrual.DDD.Utils.Data.DbContexts
{
    public class AkrualContext : DbContext, IDisposable, IAkrualContext
    { 
        internal bool disposed;

        protected AkrualContext(
            DbContextOptions<AkrualContext> options
        ) : base(options)
        {
            Changed += SetDatabaseAsDisposed;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return result;
        }

        public virtual IQueryable<TEntity> FindBy<TEntity>(Expression<Func<TEntity, bool>> selector)
            where TEntity : class, IAggregateRoot
        {
            return Set<TEntity>().Where(selector);
        }

        public virtual IQueryable<TEntity> All<TEntity>(bool forceCaching = false) where TEntity : class, IAggregateRoot
        {
            return Set<TEntity>().AsQueryable();
        }

        public virtual void AddOrUpdate<TEntity>(TEntity entity) where TEntity : class, IAggregateRoot
        {
            if (entity.Id != default(Guid) && entity.Id != new Guid())
                Set<TEntity>().Update(entity);
            else
                Set<TEntity>().Add(entity);
        }
        
        public virtual void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot
        {
            Set<TEntity>().AddRange(entities);
        }

        public virtual void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot
        {
            Set<TEntity>().UpdateRange(entities);
        }

        public virtual void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot
        {
            Set<TEntity>().RemoveRange(entities);
        }

        public virtual void Remove<TEntity>(TEntity entity) where TEntity : class, IAggregateRoot
        {
            Set<TEntity>().Remove(entity);
        }

     
        /// <summary>
        ///     use with sampleEntry when you do not know exactly which Class you are looking for in the DB.
        ///     (e.g.: if you do not know how to write the function with it's required class GetById
        ///     <Operacao>, then use with the sample;)
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="id"></param>
        /// <param name="mustReturnFromDB"></param>
        /// <param name="sampleEntry"></param>
        /// <returns></returns>
        public virtual async Task<TEntry> GetByIdAsync<TEntry>(Guid id, bool forceCaching = false,
            bool mustReturnFromDB = false)
            where TEntry : class, IAggregateRoot
        {
            var dbSet = Set<TEntry>();
            var currentEntry = await dbSet.FindAsync(id);

            if (!mustReturnFromDB) return currentEntry;
            //
            //Always from DB storage.
            //

            //Save entry's current DBC state.
            var currentState = Entry(currentEntry).State;

            //Dispose entry from current DBC
            Entry(currentEntry).State = EntityState.Detached;

            //Get from DB, not DBC
            var storedEntry = await dbSet.FindAsync(id);

            //Dispose from DBC.
            Entry(storedEntry).State = EntityState.Detached;

            //Restore currentEntry's DBC state.
            Entry(currentEntry).State = currentState;

            return storedEntry;
        }

        public void Attach<T>(T op) where T : class, IAggregateRoot
        {
            base.Set<T>().Attach(op);
        }

        public void Detach<TEntry>(TEntry entry) where TEntry : class, IAggregateRoot
        {
            base.Entry(entry).State = EntityState.Detached;
        }

        public void UpdateManyToMany<TEntry, TKey>(IEnumerable<TEntry> currentItems, IEnumerable<TEntry> newItems, Func<TEntry, TKey> getKey)
            where TEntry : class, IAggregateRoot
        {
            if(currentItems.IsNullOrEmpty())
            {
                currentItems = newItems;
                AddRange(currentItems);
            }
            else
            {
                RemoveRange(currentItems.Except(newItems, getKey));
                AddRange(newItems.Except(currentItems, getKey));
            }
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        internal event EventHandler Changed;

       

        internal void SetDatabaseAsDisposed(object sender, EventArgs e)
        {
            disposed = true;
        }
    }
}