using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    public class FactoryCreationExecutingContext<TAggregate, T> where TAggregate : AggregateRoot<T>
    {
        public TAggregate ObjectBeingCreated { get; set; }
    }
}
