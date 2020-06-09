using Akrual.DDD.Utils.Internal.Serializer;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Akrual.DDD.Utils.Domain.Cache
{
    public static class Redis
    {
        static long lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        static DateTimeOffset firstError = DateTimeOffset.MinValue;
        static DateTimeOffset previousError = DateTimeOffset.MinValue;

        static object reconnectLock = new object();

        // In general, let StackExchange.Redis handle most reconnects, 
        // so limit the frequency of how often this will actually reconnect.
        public static TimeSpan ReconnectMinFrequency = TimeSpan.FromSeconds(60);

        // if errors continue for longer than the below threshold, then the 
        // multiplexer seems to not be reconnecting, so re-create the multiplexer
        public static TimeSpan ReconnectErrorThreshold = TimeSpan.FromSeconds(30);

        static Lazy<ConnectionMultiplexer> multiplexer = CreateMultiplexer();

        public static ConnectionMultiplexer Connection { get { return multiplexer.Value; } }

        public static string clientName = System.Environment.GetEnvironmentVariable("client", EnvironmentVariableTarget.Process).ToString();


        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.  
        /// NOTES: 
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnect()
        ///     2. Don't call ForceReconnect for Timeouts, just for RedisConnectionExceptions or SocketExceptions
        ///     3. Call this method every time you see a connection exception, the code will wait to reconnect:
        ///         a. for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinFrequency"

        /// </summary>    
        public static void ForceReconnect()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var previousTicks = Interlocked.Read(ref lastReconnectTicks);
            var previousReconnect = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            var elapsedSinceLastReconnect = utcNow - previousReconnect;

            // If mulitple threads call ForceReconnect at the same time, we only want to honor one of them.
            if (elapsedSinceLastReconnect > ReconnectMinFrequency)
            {
                lock (reconnectLock)
                {
                    utcNow = DateTimeOffset.UtcNow;
                    elapsedSinceLastReconnect = utcNow - previousReconnect;

                    if (firstError == DateTimeOffset.MinValue)
                    {
                        // We haven't seen an error since last reconnect, so set initial values.
                        firstError = utcNow;
                        previousError = utcNow;
                        return;
                    }

                    if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                        return; // Some other thread made it through the check and the lock, so nothing to do.

                    var elapsedSinceFirstError = utcNow - firstError;
                    var elapsedSinceMostRecentError = utcNow - previousError;

                    var shouldReconnect =
                        elapsedSinceFirstError >= ReconnectErrorThreshold   // make sure we gave the multiplexer enough time to reconnect on its own if it can
                        && elapsedSinceMostRecentError <= ReconnectErrorThreshold; //make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

                    // Update the previousError timestamp to be now (e.g. this reconnect request)
                    previousError = utcNow;

                    if (shouldReconnect)
                    {
                        firstError = DateTimeOffset.MinValue;
                        previousError = DateTimeOffset.MinValue;

                        var oldMultiplexer = multiplexer;
                        CloseMultiplexer(oldMultiplexer);
                        multiplexer = CreateMultiplexer();
                        Interlocked.Exchange(ref lastReconnectTicks, utcNow.UtcTicks);
                    }
                }
            }
        }

        private static Lazy<ConnectionMultiplexer> CreateMultiplexer()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                ThreadPool.SetMinThreads(500, 500);

                string RedisCacheAddress = System.Environment.GetEnvironmentVariable("RedisCacheAddress", EnvironmentVariableTarget.Process).ToString();
                string RedisCachePort = System.Environment.GetEnvironmentVariable("RedisCachePort", EnvironmentVariableTarget.Process).ToString();
                string RedisCachePass = System.Environment.GetEnvironmentVariable("RedisCachePass", EnvironmentVariableTarget.Process).ToString();
                string client = System.Environment.GetEnvironmentVariable("client", EnvironmentVariableTarget.Process).ToString();
                string RedisCacheConnStr = System.Environment.GetEnvironmentVariable("RedisCacheConnStr", EnvironmentVariableTarget.Process).ToString();


                var connConfig = ConfigurationOptions.Parse(RedisCacheConnStr);
                connConfig.AbortOnConnectFail = false;
                connConfig.Ssl = true;
                connConfig.ConnectRetry = 3;
                connConfig.ConnectTimeout = 20000;
                connConfig.SyncTimeout = 5000;
                connConfig.DefaultDatabase = 0;

                return ConnectionMultiplexer.Connect(connConfig);
            });
        }

        
        private static void CloseMultiplexer(Lazy<ConnectionMultiplexer> oldMultiplexer)
        {
            if (oldMultiplexer != null)
            {
                try
                {
                    oldMultiplexer.Value.Close();
                }
                catch (Exception)
                {
                    // Example error condition: if accessing old.Value causes a connection attempt and that fails.
                }
            }
        }


        public static async Task<T> RunMethodWithRetriesAsync<T>(Func<IDatabase, Func<Task<T>>> fetcherFunction)
        {
            while (true)
            {
                try
                {
                    var database = Redis.Connection.GetDatabase();
                    return await fetcherFunction(database)();
                }
                catch (RedisConnectionException redisEx)
                {
                    try
                    {
                        Redis.ForceReconnect();
                    }
                    catch (ObjectDisposedException objEx)
                    {
                    }
                    continue;
                }
                catch (System.Net.Sockets.SocketException sockEx)
                {
                    try
                    {
                        Redis.ForceReconnect();
                    }
                    catch (ObjectDisposedException objEx)
                    {
                    }
                    continue;
                }
                catch (ObjectDisposedException objEx)
                {
                    continue;
                }
                break;
            }
            return default(T);
        }


        static Lazy<MessagePackSerializerLz4> serializer = CreateSerializer();
        
        private static Lazy<MessagePackSerializerLz4> CreateSerializer()
        {
            return new Lazy<MessagePackSerializerLz4>(() =>
            {
                return new MessagePackSerializerLz4();
            });
        }

        public static async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
            where T : IConcurrencyCheckable
        {
            var oldConcurrencyGuid = value.ConcurrencyGuid;

            var newConcurrencyGuid = Guid.NewGuid();
            value.ConcurrencyGuid = newConcurrencyGuid;

            var entryBytes = serializer.Value.Serialize(value);

            var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

            Func<IDatabase, Func<Task<bool>>> f = (database) =>
            {
                Func<Task<bool>> f2 = async () =>
                {
                    var tran = database.CreateTransaction();
                    if(oldConcurrencyGuid != default(Guid))
                    {
                        tran.AddCondition(Condition.StringEqual(key+"_ccid", oldConcurrencyGuid.ToString("D")));
                    }
                    await tran.StringSetAsync(clientName + key, entryBytes, expiration, when, flag);
                    await tran.StringSetAsync(key+"_ccid", value.ConcurrencyGuid.ToString("D"));
                    return await tran.ExecuteAsync();
                }; 

                return f2;
            };

            return await RunMethodWithRetriesAsync(f);
        }
        

        public static async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = items.Select(s => new KeyValuePair<RedisKey, RedisValue>(clientName + s.Item1, serializer.Value.Serialize(s.Item2))).ToArray();

            var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

            Func<IDatabase, Func<Task<bool>>> f = (database) =>
            {
                Func<Task<bool>> f2 = async () =>
                {
                    var result = await database.StringSetAsync(values, when, flag);

                    var tasks = new Task[values.Length];

                    for (var i = 0; i < values.Length; i++)
                        tasks[i] = database.KeyExpireAsync(values[i].Key, expiresAt.UtcDateTime, flag);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    return result;
                }; 

                return f2;
            };

            return await RunMethodWithRetriesAsync(f);
        }

        public static async Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            
            Func<IDatabase, Func<Task<RedisValue>>> f = (database) =>
            {
                Func<Task<RedisValue>> f2 = async () =>
                {
                    return await database.StringGetAsync(clientName + key, flag).ConfigureAwait(false);
                }; 

                return f2;
            };

            var valueBytes =  await RunMethodWithRetriesAsync(f);

            if (!valueBytes.HasValue)
                return default;


            return serializer.Value.Deserialize<T>(valueBytes);
        }


        public static async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            var redisKeys = keys.Select(x => (RedisKey)(clientName + x)).ToArray();

            Func<IDatabase, Func<Task<RedisValue[]>>> f = (database) =>
            {
                Func<Task<RedisValue[]>> f2 = async () =>
                {
                    return await database.StringGetAsync(redisKeys, flag).ConfigureAwait(false);
                }; 

                return f2;
            };

            var result =  await RunMethodWithRetriesAsync(f);


            var dict = new Dictionary<string, T>(redisKeys.Length, StringComparer.Ordinal);

            for (var index = 0; index < redisKeys.Length; index++)
            {
                var value = result[index];
                dict.Add(redisKeys[index], value == RedisValue.Null ? default : serializer.Value.Deserialize<T>(value));
            }

            return dict;
        }
    }
}
