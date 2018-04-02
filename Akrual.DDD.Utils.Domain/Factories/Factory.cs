using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Utils.Collections;
using Akrual.DDD.Utils.Domain.Utils.UUID;

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
        public abstract Task<TAggregate> CreateDefaultInstance(Guid guid);

        /// <summary>
        /// Creates the Aggregate with all the invariants Checked
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public virtual async Task<TAggregate> Create(Guid guid)
        {
            // Create default Aggregate
            var aggregate = await CreateDefaultInstance(guid);

            var creationContext = new FactoryCreationExecutingContext<TAggregate, T>
            {
                ObjectBeingCreated = aggregate
            };

            AggregateCreation(creationContext);

            return aggregate;
        }
    }
}
