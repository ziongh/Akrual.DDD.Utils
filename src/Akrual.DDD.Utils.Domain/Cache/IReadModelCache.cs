using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Akrual.DDD.Utils.Domain.Cache
{
    public interface IReadModelCache
    {
        Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
            where T : IConcurrencyCheckable;


        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);


        Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None);


        Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None);
    }


    
    public class MockReadModelCache : IReadModelCache
    {
        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return true;
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None) where T : IConcurrencyCheckable
        {
            return true;
        }

        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            return null;
        }

        public async Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            return default(T);
        }
    }

    public class RedisReadModelCache : IReadModelCache
    {
        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return await Redis.AddAllAsync(items, expiresAt, when,flag);
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None) where T : IConcurrencyCheckable
        {
            return await Redis.AddAsync(key, value, expiresAt, when,flag);
        }

        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            return await Redis.GetAllAsync<T>(keys, flag);
        }

        public async Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            return await Redis.GetAsync<T>(key, flag);
        }
    }
}
