using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    public class FactoryCreationExecutingContext<TAggregate, T> 
        where TAggregate : IAggregateRoot
    {
        public TAggregate ObjectBeingCreated { get; set; }
    }
}
