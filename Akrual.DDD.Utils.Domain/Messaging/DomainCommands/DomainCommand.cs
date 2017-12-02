﻿using System;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>
    /// Base class for implementing domain events that represent changes in a domain entity.
    /// </summary>
    public abstract class DomainCommand : BaseMessaging<DomainCommand>, IDomainCommand
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