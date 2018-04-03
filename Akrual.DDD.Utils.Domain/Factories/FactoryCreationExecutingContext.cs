using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    /// <summary>
    /// Used to Pass information to the Factory. So we can alter the behaviour of the factory.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="T"></typeparam>
    public sealed class FactoryCreationExecutingContext<TAggregate, T> 
        where TAggregate : IAggregateRoot
    {
        /// <summary>
        /// Is the object being created. So if there is a need to change how 
        /// it is created or some internal property use it
        /// </summary>
        public TAggregate ObjectBeingCreated { get; set; }
    }
}
