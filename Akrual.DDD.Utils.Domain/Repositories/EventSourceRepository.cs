using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories;

namespace Akrual.DDD.Utils.Domain.Repositories
{
    public class EventSourceRepository<T> : IRepository<T> where T : class, IAggregateRoot
    {
        private readonly IFactory<T> _factory;
        private readonly IEventStore _eventStore;

        public EventSourceRepository(IFactory<T> factory, IEventStore eventStore)
        {
            _factory = factory;
            _eventStore = eventStore;
        }

        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order. Then it check all later events that should be applied before the given
        ///     point and applies them too.
        ///     <remarks> 
        ///         In other words, it will give the result of a particular time point within the stream (everything we know 
        ///         that happened until that point in time, counting with events that may have happened later, but affects some point back in time) 
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public async Task<T> CreateAsOf(Guid guid, DateTime? AsOfDate = null)
        {
            var aggregate = await _factory.Create(guid);

            aggregate = await RebuildAggregateFromEventStreamAsOf(aggregate, AsOfDate);

            return aggregate;
        }

        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order.
        ///     <remarks> 
        ///         In other words, it will give the result at a particular time point within the stream (what did we know as at this point in time) 
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public async Task<T> CreateAsAt(Guid guid, DateTime? AsAtDate = null)
        {
            var aggregate = await _factory.Create(guid);

            aggregate = await RebuildAggregateFromEventStreamAsAt(aggregate, AsAtDate);

            return aggregate;
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
