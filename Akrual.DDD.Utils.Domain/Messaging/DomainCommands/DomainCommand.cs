using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>
    /// Base class for implementing domain Commands that represent the will for something to happen.
    /// It by default expect the handler to return an IEnumerable&lt;DomainEvent&gt;
    /// </summary>
    public abstract class DomainCommand : BaseMessaging<DomainCommand>, IDomainCommand<IEnumerable<IDomainEvent>>
    {
        /// <summary>
        /// Gets the Aggregate Root id.
        /// </summary>
        public Guid AggregateRootId { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DomainCommand"/> class.
        /// </summary>
        /// <param name="aggregateRootId">Aggregate Root id.</param>
        /// <param name="entityVersion">Entity instance version.</param>
        protected DomainCommand(Guid aggregateRootId, long entityVersion)
        {
            if (aggregateRootId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Entity id must be defined.", "aggregateRootId");
            }

            AggregateRootId = aggregateRootId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainCommand"/> class.
        /// </summary>
        /// <param name="aggregateRootId">Aggregate Root instance id.</param>
        protected DomainCommand(Guid aggregateRootId)
            : this(aggregateRootId, 0)
        {
        }
    }
}
