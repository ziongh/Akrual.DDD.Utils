using System;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using MessagePack;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>
    /// Base class for implementing domain Commands that represent the will for something to happen.
    /// It by default expect the handler to return an IEnumerable&lt;DomainEvent&gt;
    /// </summary>
    [MessagePackObject]
    public abstract class DomainCommand : BaseMessaging<DomainCommand>, IDomainCommand
    {
        /// <summary>
        /// Gets the Aggregate Root id.
        /// </summary>
        [Key(0)]
        public Guid AggregateRootId { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainCommand"/> class.
        /// </summary>
        /// <param name="aggregateRootId">Aggregate Root id.</param>
        /// <param name="sagaId">Saga Id to identify WorkFlow.</param>
        [SerializationConstructor]
        protected DomainCommand(Guid aggregateRootId, Guid sagaId)
        {
            if (aggregateRootId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Entity id must be defined.", "aggregateRootId");
            }

            AggregateRootId = aggregateRootId;
            SagaId = sagaId;
            TimeStamp = DateTimeProvider.Current.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainCommand"/> class.
        /// </summary>
        /// <param name="aggregateRootId">Aggregate Root instance id.</param>
        protected DomainCommand(Guid aggregateRootId)
            : this(aggregateRootId, Guid.NewGuid())
        {
        }
        
        [Key(5)]
        public DateTime TimeStamp { get; protected set; }
    }
}
