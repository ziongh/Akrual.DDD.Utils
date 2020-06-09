using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Cache;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories;

namespace Akrual.DDD.Utils.Domain.Repositories
{
    public class EventSourceRepository<T> : IRepository<T> where T : class, IAggregateRoot
    {
        private readonly IFactory<T> _factory;
        private readonly IEventStore _eventStore;
        private readonly IReadModelCache _readModelCache;

        public EventSourceRepository(IFactory<T> factory, IEventStore eventStore, IReadModelCache readmodelCache)
        {
            _factory = factory;
            _eventStore = eventStore;
            this._readModelCache = readmodelCache;
        }

        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order. Then it check all later events that should be applied before the given
        ///     point and applies them too.
        ///     <remarks> 
        ///         In other words, it will give the result of a particular time point within the stream (everything we know 
        ///         that happened until that point in time, counting with events that may have happened later, but affects some point back in time) 
        ///         If you use cache it will return the last image, ignoring as at.
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public async Task<T> CreateAsOf(Guid guid, DateTime? AsOfDate = null, bool useCache = true)
        {
            var aggregate = await _factory.Create(guid);

            if (useCache)
            {
                aggregate = await CreateAsOfInternal(aggregate, AsOfDate);
            }
            else
            {
                aggregate = await RebuildAggregateFromEventStreamAsOf(aggregate, AsOfDate);
            }

            return aggregate;
        }


        private async Task<T> CreateAsOfInternal(T aggregate, DateTime? AsOfDate = null, int count = 1)
        {
            if(count > 8) return null;
            var cachedEntry = await _readModelCache.GetAsync<T>(aggregate.StreamBaseName + "_" + aggregate.Id.ToString("D"));

            if(cachedEntry == null)
            {
                // Fetch from Event Store
                cachedEntry = await RebuildAggregateFromEventStreamAsOf(aggregate, AsOfDate);

                if(!await _readModelCache.AddAsync(aggregate.StreamBaseName + "_" + cachedEntry.Id.ToString("D"), cachedEntry, DateTime.UtcNow.AddDays(7), StackExchange.Redis.When.NotExists))
                {
                    return await CreateAsOfInternal(aggregate, AsOfDate, count++);
                }
            }

            return cachedEntry;
        }



        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order.
        ///     <remarks> 
        ///         In other words, it will give the result at a particular time point within the stream (what did we know as at this point in time) 
        ///         If you use cache it will return the last image, ignoring as at.
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public async Task<T> CreateAsAt(Guid guid, DateTime? AsAtDate = null, bool useCache = true)
        {
            var aggregate = await _factory.Create(guid);

            if (useCache)
            {
                aggregate = await CreateAsAtInternal(aggregate, AsAtDate);
            }
            else
            {
                aggregate = await RebuildAggregateFromEventStreamAsAt(aggregate, AsAtDate);
            }

            return aggregate;
        }


        private async Task<T> CreateAsAtInternal(T aggregate, DateTime? AsAtDate = null, int count = 1)
        {
            if(count > 8) return null;
            var cachedEntry = await _readModelCache.GetAsync<T>(aggregate.StreamBaseName + "_" + aggregate.Id.ToString("D"));

            if(cachedEntry == null)
            {
                // Fetch from Event Store
                cachedEntry = await RebuildAggregateFromEventStreamAsAt(aggregate, AsAtDate);

                if(!await _readModelCache.AddAsync(aggregate.StreamBaseName + "_" + cachedEntry.Id.ToString("D"), cachedEntry, DateTime.UtcNow.AddDays(7), StackExchange.Redis.When.NotExists))
                {
                    return await CreateAsOfInternal(aggregate, AsAtDate, count++);
                }
            }

            return cachedEntry;
        }


        private async Task<T> RebuildAggregateFromEventStreamAsAt(T result, DateTime? AsAtDate = null)
        {
            var stream = await _eventStore.GetEventStream(result);
            if (stream != null)
            {
                foreach (var recordedEvent in await stream.Events)
                {
                    var @event = recordedEvent.Event;
                    if (AsAtDate == null || recordedEvent.CreatedAt <= AsAtDate)
                    {
                        await result.ApplyOneEvent((dynamic)@event, new Domain.Aggregates.Internal());
                    }
                }
            }

            result.AllEventsStored();
            return result;
        }

        private async Task<T> RebuildAggregateFromEventStreamAsOf(T result, DateTime? AsOfDate = null)
        {
            var stream = await _eventStore.GetEventStream(result);
            if (stream != null)
            {
                foreach (var recordedEvent in await stream.Events)
                {
                    var @event = recordedEvent.Event;
                    if (AsOfDate == null || recordedEvent.CreatedAt <= AsOfDate)
                    {
                        await result.ApplyOneEvent((dynamic)@event, new Domain.Aggregates.Internal());
                    }
                    else if (@event.AppliesAt <= AsOfDate)
                    {
                        await result.ApplyOneEvent((dynamic)@event, new Domain.Aggregates.Internal());
                    }
                }
            }

            result.AllEventsStored();
            return result;
        }
    }
}
