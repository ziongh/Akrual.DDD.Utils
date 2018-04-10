﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Entities;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Internal.ConcurrentLists;
using Akrual.DDD.Utils.Internal.Contracts;
using Akrual.DDD.Utils.Internal.Extensions;
using Akrual.DDD.Utils.Internal.Logging;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Domain.Aggregates
{
    public interface IAggregateRoot : IEntity
    {
        /// <summary>
        /// The number of events loaded into this aggregate.
        /// </summary>
        Counter EventsLoaded { get; }

        /// <summary>
        /// Gets all domain events that have been applied to the aggregate root instance.
        /// </summary>
        /// <returns>A collection of domain events.</returns>
        IEnumerable<IDomainEvent> GetEventStream();

        /// <summary>
        /// Enuerates the supplied events and applies them in order to the aggregate.
        /// </summary>
        /// <param name="domainEvents"></param>
        Task<IEnumerable<IMessaging>> ApplyEvents(IEnumerable<IDomainEvent> domainEvents);

        /// <summary>
        /// Enuerates the supplied events and applies them in order to the aggregate.
        /// </summary>
        /// <param name="domainEvents"></param>
        Task<IEnumerable<IMessaging>> ApplyEvents(params IDomainEvent[] domainEvents);

        /// <summary>
        /// Applies a single event to the aggregate.
        /// <remarks>Normally thuis method should not be used. Because, the Aggregate
        /// when implementing one IApplyDomainEvent, will be automatically callede when an event is published.</remarks>
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="ev"></param>
        Task<IEnumerable<IMessaging>> ApplyOneEvent<TEvent>(TEvent ev)
            where TEvent : IDomainEvent;
    }

    /// <summary>
    ///     Base class for implementing aggregate root domain objects.
    ///     <remarks><c>No external code should access the internal objects of this Aggregate!</c></remarks>
    ///     <remarks><c>So add properties as private or maximum internal only!</c></remarks>
    /// </summary>
    public abstract class AggregateRoot<T> : Entity<T>, IAggregateRoot where T : new()
    {
        internal static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ConcurrentList<IDomainEvent> _changes = new ConcurrentList<IDomainEvent>();

        private readonly ConcurrentList<IDomainEvent> eventStream;

        /// <summary>
        /// The number of events loaded into this aggregate.
        /// </summary>
        public Counter EventsLoaded { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
        /// Every constructor have to satisfy:
        ///     <remarks><c>GUID. This must be unique in the whole Application.!</c></remarks>
        ///     <remarks><c>You should probably extend this constructor to include every property that makes this entity unique.</c></remarks>
        /// </summary>
        /// <param name="id">Aggregate root instance id.</param>
        protected AggregateRoot(Guid id) : base(id,null)
        {
            eventStream = new ConcurrentList<IDomainEvent>();
            EventsLoaded = new Counter();
        }


        /// <summary>
        /// Gets all domain events that have been applied to the aggregate root instance.
        /// </summary>
        /// <returns>A collection of domain events.</returns>
        public virtual IEnumerable<IDomainEvent> GetEventStream()
        {
            return eventStream;
        }

        /// <summary>
        /// Enuerates the supplied events and applies them in order to the aggregate.
        /// </summary>
        /// <param name="domainEvents"></param>
        public async Task<IEnumerable<IMessaging>> ApplyEvents(IEnumerable<IDomainEvent> domainEvents)
        {
            var listOfMessages = new List<IMessaging>();

            domainEvents.EnsuresNotNullOrEmpty();
            foreach (var e in domainEvents)
            {
                eventStream.Add(e);
                listOfMessages.AddRange(await ApplyOneEvent((dynamic)e));
            }

            return listOfMessages;
        }

        /// <summary>
        /// Enuerates the supplied events and applies them in order to the aggregate.
        /// </summary>
        /// <param name="domainEvents"></param>
        public async Task<IEnumerable<IMessaging>> ApplyEvents(params IDomainEvent[] domainEvents)
        {
            var listOfMessages = new List<IMessaging>();
            domainEvents.EnsuresNotNullOrEmpty();
            foreach (var e in domainEvents)
            {
                eventStream.Add(e);
                listOfMessages.AddRange(await ApplyOneEvent((dynamic)e));
            }

            return listOfMessages;
        }

        /// <summary>
        /// Applies a single event to the aggregate.
        /// <remarks>Normally thuis method should not be used. Because, the Aggregate
        /// when implementing one IApplyDomainEvent, will be automatically callede when an event is published.</remarks>
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="ev"></param>
        public async Task<IEnumerable<IMessaging>> ApplyOneEvent<TEvent>(TEvent ev)
            where TEvent : IDomainEvent
        {
            var applier = this as IHandleDomainEvent<TEvent>;
            if (applier == null)
                throw new InvalidOperationException(string.Format(
                    "Aggregate {0} does not know how to apply event {1}",
                    GetType().Name, ev.GetType().Name));
            var messages = await applier.Handle(ev, CancellationToken.None);
            EventsLoaded.NextValue();
            return messages;
        }
    }
}
