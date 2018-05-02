using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    public abstract class FactoryBase<T> : IFactory<T>
        where T : IAggregateRoot
    {
        private readonly List<EventHandler<FactoryCreationExecutingContext<T, T>>>
            _aggregateCreation;
        
        public event EventHandler<FactoryCreationExecutingContext<T, T>> OnAfterCreateDefaultInstance
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
        

        public virtual void AggregateCreation(FactoryCreationExecutingContext<T, T> args)
        {
            lock (_aggregateCreation)
            {
                foreach (var onAggregateCreator in _aggregateCreation)
                {
                    onAggregateCreator(this, args);
                }
            }
        }

        protected FactoryBase()
        {
            _aggregateCreation = new List<EventHandler<FactoryCreationExecutingContext<T, T>>>();
        }

        /// <summary>
        ///     Creates the default Aggregate. It should be already filled with the UUID.
        ///     <remarks><c>Use GuidGenerator to generate UUID!</c></remarks>
        /// </summary>
        protected internal abstract Task<T> CreateDefaultInstance(Guid guid);
        
         
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
        public virtual async Task<T> Create(Guid guid)
        {
            // Create default Aggregate
            var aggregate = await CreateDefaultInstance(guid);

            var creationContext = new FactoryCreationExecutingContext<T, T>
            {
                ObjectBeingCreated = aggregate
            };

            AggregateCreation(creationContext);

            return aggregate;
        }
    }
}
