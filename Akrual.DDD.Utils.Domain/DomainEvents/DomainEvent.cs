﻿using System;

namespace Akrual.DDD.Utils.Domain.DomainEvents
{
    /// <summary>
    /// Base class for implementing domain events that represent changes in a domain entity.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the entity id.
        /// </summary>
        public Guid EntityId { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="entityId">Entity instance id.</param>
        /// <param name="entityVersion">Entity instance version.</param>
        protected DomainEvent(Guid entityId, long entityVersion)
        {
            if (entityId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Entity id must be defined.", "entityId");
            }

            EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="entityId">Entity instance id.</param>
        protected DomainEvent(Guid entityId)
            : this(entityId, 0)
        {
        }
    }
}
