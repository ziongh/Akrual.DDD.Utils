using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    public abstract class Factory<TAggregate,T> 
        where TAggregate : IAggregateRoot
    {
        private readonly List<EventHandler<FactoryCreationExecutingContext<TAggregate, T>>>
            _aggregateCreation;
        
        public event EventHandler<FactoryCreationExecutingContext<TAggregate, T>> OnAfterCreateDefaultInstance
        {
            add
            {
                lock (_aggregateCreation)
                {
                    _aggregateCreation.Add(value);
                }
            }
            remove
            {
                lock (_aggregateCreation)
                {
                    _aggregateCreation.Remove(value);
                }
            }
        }

        public virtual void AggregateCreation(FactoryCreationExecutingContext<TAggregate, T> args)
        {
            lock (_aggregateCreation)
            {
                foreach (var onAggregateCreator in _aggregateCreation)
                {
                    onAggregateCreator(this, args);
                }
            }
        }

        protected Factory()
        {
            _aggregateCreation = new List<EventHandler<FactoryCreationExecutingContext<TAggregate, T>>>();
        }

        /// <summary>
        ///     Creates the default Aggregate. It should be already filled with the UUID.
        ///     <remarks><c>Use GuidGenerator to generate UUID!</c></remarks>
        /// </summary>
        protected internal abstract Task<TAggregate> CreateDefaultInstanceAsOf(Guid guid, DateTime? AsOfDate = null);

        /// <summary>
        ///     Creates the default Aggregate. It should be already filled with the UUID.
        ///     <remarks><c>Use GuidGenerator to generate UUID!</c></remarks>
        /// </summary>
        protected internal abstract Task<TAggregate> CreateDefaultInstanceAsAt(Guid guid, DateTime? AsOAtDate = null);

         
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
        public virtual async Task<TAggregate> CreateAsOf(Guid guid, DateTime? AsOfDate = null)
        {
            // Create default Aggregate
            var aggregate = await CreateDefaultInstanceAsOf(guid, AsOfDate);

            var creationContext = new FactoryCreationExecutingContext<TAggregate, T>
            {
                ObjectBeingCreated = aggregate
            };

            AggregateCreation(creationContext);

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
        public virtual async Task<TAggregate> CreateAsAt(Guid guid, DateTime? AsAtDate = null)
        {
            // Create default Aggregate
            var aggregate = await CreateDefaultInstanceAsAt(guid, AsAtDate);

            var creationContext = new FactoryCreationExecutingContext<TAggregate, T>
            {
                ObjectBeingCreated = aggregate
            };

            AggregateCreation(creationContext);

            return aggregate;
        }
    }
}
