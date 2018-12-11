using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.DbContexts;
using Akrual.DDD.Utils.Domain.Repositories;
using Akrual.DDD.Utils.Internal.Extensions;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace Akrual.DDD.Utils.Data.DbContexts
{
    public class FakeAppContext : IAkrualContext
    {
        Dictionary<Type, object> database = new Dictionary<Type, object>();
        private readonly object _lock = new object();

        public FakeAppContext(params Assembly[] assemblies)
        {
            List<Type> typesToRegister = new List<Type>();
            foreach (var assembly in assemblies)
            {
                typesToRegister.AddRange(assembly
                    .GetTypes()
                    .Where(type => type.Implements(typeof(IAggregateRoot))));
            }

            //dynamically load all configurations in assemblies

            foreach (var typeofEntity in typesToRegister)
            {
                if (typeofEntity != null)
                {
                    var DbAsyncEnumerableOfType =  typeof(DbAsyncEnumerableExtensions).GetMember("CreateDbAsyncEnumerable").OfType<MethodInfo>()
                        .Single(x => x.IsGenericMethod)
                        .MakeGenericMethod(typeofEntity)
                        .Invoke(null, new object[] {});

                    database.Add(typeofEntity, DbAsyncEnumerableOfType);
                }
            }
        }

        public void Dispose()
        {
        }

        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            return 0;
        }

        public IQueryable<TEntity> FindBy<TEntity>(Expression<Func<TEntity, bool>> selector) where TEntity : class, IAggregateRoot
        {
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntity), out listOfEntities)) return new DbAsyncEnumerable<TEntity>(new List<TEntity>());

            var castedList = listOfEntities as DbAsyncEnumerable<TEntity>;
            if (castedList == null)
            {
                return new DbAsyncEnumerable<TEntity>(new List<TEntity>());
            }

            return castedList.Where(selector);
        }

        public IQueryable<TEntity> FindByCacheable<TEntity>(Expression<Func<TEntity, bool>> selector) where TEntity : class, IAggregateRoot
        {
            return FindBy(selector);
        }

        public IQueryable<TEntity> All<TEntity>(bool forceCaching = false) where TEntity : class, IAggregateRoot
        {
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntity), out listOfEntities)) return new DbAsyncEnumerable<TEntity>(new List<TEntity>());

            var castedList = listOfEntities as DbAsyncEnumerable<TEntity>;
            if (castedList == null)
            {
                return new DbAsyncEnumerable<TEntity>(new List<TEntity>());
            }

            return castedList;
        }
        

        public void AddOrUpdate<TEntity>(TEntity entity) where TEntity : class, IAggregateRoot
        {
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntity), out listOfEntities)) return;

            var castedList = listOfEntities as IEnumerable<TEntity>;
            if (castedList == null)
            {
                return;
            }

            var items = castedList.ToList();
            items.RemoveAll(s => s.Id == entity.Id);
            items.Add(entity);
            var newList = new DbAsyncEnumerable<TEntity>(items);
            database.Remove(typeof(TEntity));
            database.Add(typeof(TEntity), newList);
        }

        public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot
        {
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntity), out listOfEntities)) return;

            var castedList = listOfEntities as IEnumerable<TEntity>;
            if (castedList == null)
            {
                return;
            }

            var items = castedList.ToList();
            items.AddRange(entities);
            var newList = new DbAsyncEnumerable<TEntity>(items);
            database.Remove(typeof(TEntity));
            database.Add(typeof(TEntity), newList);
        }

        public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot
        {
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntity), out listOfEntities)) return;

            var castedList = listOfEntities as IEnumerable<TEntity>;
            if (castedList == null)
            {
                return;
            }

            var ids = entities.Select(s => s.Id).ToList();
            var items = castedList.ToList();
            items.RemoveAll(s => ids.Contains(s.Id));
            items.AddRange(entities);
            var newList = new DbAsyncEnumerable<TEntity>(items);
            database.Remove(typeof(TEntity));
            database.Add(typeof(TEntity), newList);
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IAggregateRoot
        {
           
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntity), out listOfEntities)) return;

            var castedList = listOfEntities as IEnumerable<TEntity>;
            if (castedList == null)
            {
                return;
            }

            var ids = entities.Select(s => s.Id).ToList();
            var items = castedList.ToList();
            items.RemoveAll(s => ids.Contains(s.Id));
            var newList = new DbAsyncEnumerable<TEntity>(items);
            database.Remove(typeof(TEntity));
            database.Add(typeof(TEntity), newList);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class, IAggregateRoot
        {
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntity), out listOfEntities)) return;

            var castedList = listOfEntities as IEnumerable<TEntity>;
            if (castedList == null)
            {
                return;
            }
            
            var items = castedList.ToList();
            items.RemoveAll(s => s.Id == entity.Id);
            var newList = new DbAsyncEnumerable<TEntity>(items);
            database.Remove(typeof(TEntity));
            database.Add(typeof(TEntity), newList);
        }

        public async Task<TEntry> GetByIdAsync<TEntry>(Guid id, bool forceCaching = false, bool mustReturnFromDB = false) where TEntry : class, IAggregateRoot
        {
            object listOfEntities;
            if (!database.TryGetValue(typeof(TEntry), out listOfEntities)) return default(TEntry);

            var castedList = listOfEntities as IEnumerable<TEntry>;
            if (castedList == null)
            {
                return default(TEntry);
            }

            return castedList.FirstOrDefault(s => s.Id == id);
        }
        
        

        public void Attach<TEntry>(TEntry op) where TEntry : class, IAggregateRoot
        {
            
        }

        public void Detach<TEntry>(TEntry entry) where TEntry : class, IAggregateRoot
        {
        }

        public void UpdateManyToMany<TEntry, TKey>(IEnumerable<TEntry> currentItems, IEnumerable<TEntry> newItems, Func<TEntry, TKey> getKey) where TEntry : class, IAggregateRoot
        {
        }

        public void ClearDB()
        {
            var listsToRemove = new List<Type>();
            var listsToAdd = new Dictionary<Type, object>();
            foreach (var entityList in database)
            {
                var list = entityList.Value as IEnumerable;
                if (list != null)
                {
                    var typeofEntity = entityList.Value.GetType().GetGenericArguments()[0];

                    var DbAsyncEnumerableOfType =  typeof(DbAsyncEnumerableExtensions).GetMember("CreateDbAsyncEnumerable").OfType<MethodInfo>()
                        .Single(x => x.IsGenericMethod)
                        .MakeGenericMethod(typeofEntity)
                        .Invoke(null, new object[] {});

                    listsToRemove.Add(entityList.Key);
                    listsToAdd.Add(entityList.Key,DbAsyncEnumerableOfType);
                }
            }

            listsToRemove.ForEach(s => database.Remove(s));
            listsToAdd.ForEach(s => database.Add(s.Key,s.Value));
        }
    }
}
