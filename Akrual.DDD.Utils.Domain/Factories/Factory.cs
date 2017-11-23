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

        public event EventHandler<FactoryCreationExecutingContext<TAggregate, T>> AggregateCreation
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

        protected virtual void OnAggregateCreation(FactoryCreationExecutingContext<TAggregate, T> args)
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

        protected abstract TAggregate CreateDefaultInstance();

        public virtual TAggregate Create()
        {
            var aggregate = CreateDefaultInstance();

            var creationContext = new FactoryCreationExecutingContext<TAggregate, T>
            {
                ObjectBeingCreated = aggregate
            };

            OnAggregateCreation(creationContext);

            return aggregate;
        }
    }
}
