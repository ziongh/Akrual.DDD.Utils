using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Utils.Collections;
using Akrual.DDD.Utils.Domain.Utils.UUID;

namespace Akrual.DDD.Utils.Domain.Factories
{
    public abstract class Factory<TAggregate,T> where TAggregate : AggregateRoot<T>
    {
        private readonly WeakCollection<EventHandler<FactoryCreationExecutingContext<TAggregate, T>>> _aggregateCreation =
            new WeakCollection<EventHandler<FactoryCreationExecutingContext<TAggregate, T>>>();

        public event EventHandler<FactoryCreationExecutingContext<TAggregate, T>> OnAggregateCreation
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

        protected virtual void AggregateCreation(FactoryCreationExecutingContext<TAggregate, T> args)
        {
            lock (_aggregateCreation)
            {
                foreach (var onAggregateCreator in _aggregateCreation.GetLiveItems())
                {
                    onAggregateCreator(this, args);
                }
            }
        }

        protected Factory()
        {
        }

        /// <summary>
        ///     Creates the default Aggregate. It should be already filled with the UUID.
        ///     <remarks><c>Use GuidGenerator to generate UUID!</c></remarks>
        /// </summary>
        protected abstract TAggregate CreateDefaultInstance();

        /// <summary>
        /// Creates the Aggregate with all the invariants Checked
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public virtual TAggregate Create()
        {
            // Create default Aggregate
            var aggregate = CreateDefaultInstance();

            var creationContext = new FactoryCreationExecutingContext<TAggregate, T>
            {
                ObjectBeingCreated = aggregate
            };

            AggregateCreation(creationContext);




            return aggregate;
        }
    }
}
