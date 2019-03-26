using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Entities;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Internal.ConcurrentLists;
using Akrual.DDD.Utils.Internal.Contracts;
using Akrual.DDD.Utils.Internal.Logging;
using Akrual.DDD.Utils.Internal.UsefulClasses;

namespace Akrual.DDD.Utils.Domain.Aggregates
{
    public interface IAggregateRoot : IEntity
    {
        /// <summary>
        /// The number of events loaded into this aggregate.
        /// </summary>
        int GetTotalEventsLoaded { get; }

        /// <summary>
        /// The number of events loaded into this aggregate by the time we finished fetchihng it from the DB.
        /// </summary>
        int GetTotalEventsLoadedFromDB { get; }

        /// <summary>
        /// Gets all domain events that have been applied to the aggregate root instance.
        /// </summary>
        /// <returns>A collection of domain events.</returns>
        IEnumerable<IDomainEvent> GetEventStream();

        /// <summary>
        /// Gets all new domain events that have been applied to the aggregate root instance since the fetching from the database.
        /// </summary>
        /// <returns>A collection of domain events.</returns>
        IEnumerable<IDomainEvent> GetChangesEventStream();

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
        /// <param name="nothing">This parameter is only here to block any acces to this method from outside of the library</param>
        Task<IEnumerable<IMessaging>> ApplyOneEvent<TEvent>(TEvent ev, Internal nothing)
            where TEvent : IDomainEvent;

        /// <summary>
        /// Notify the Aggregate that all events where Stored. And transfer all Events from Changes to EventStream.
        /// </summary>
        void AllEventsStored();
    }

    /// <summary>
    ///     Base class for implementing aggregate root domain objects.
    ///     <remarks><c>No external code should access the internal objects of this Aggregate!</c></remarks>
    ///     <remarks><c>So add properties as private or maximum internal only!</c></remarks>
    /// </summary>
    public abstract class AggregateRoot<T> : Entity<T>, IAggregateRoot where T : new()
    {
        internal static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ConcurrentList<IDomainEvent> _changes;

        private readonly ConcurrentList<IDomainEvent> eventStream;

        
        private Counter EventsLoaded { get; }

        
        private Counter EventsLoadedFromDB { get; set; }

        /// <summary>
        /// The number of events loaded into this aggregate.
        /// </summary>
        public int GetTotalEventsLoaded => EventsLoaded.GetCurrentValue();

        /// <summary>
        /// The number of events loaded into this aggregate by the time we finished fetchihng it from the DB.
        /// </summary>
        public int GetTotalEventsLoadedFromDB => EventsLoadedFromDB.GetCurrentValue();

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRoot&lt;T&gt;"/> class.
        /// Every constructor have to satisfy:
        ///     <remarks><c>GUID. This must be unique in the whole Application.!</c></remarks>
        ///     <remarks><c>You should probably extend this constructor to include every property that makes this entity unique.</c></remarks>
        /// </summary>
        /// <param name="id">Aggregate root instance id.</param>
        protected AggregateRoot(Guid id) : base(id,null)
        {
            eventStream = new ConcurrentList<IDomainEvent>();
            _changes = new ConcurrentList<IDomainEvent>();
            EventsLoaded = new Counter();
            EventsLoadedFromDB = new Counter();
        }

        /// <summary>
        /// Notify the Aggregate that all events where Stored. And transfer all Events from Changes to EventStream.
        /// </summary>
        public virtual void AllEventsStored()
        {
            eventStream.Concat(_changes); 
            _changes.Clear();
            EventsLoadedFromDB = new Counter(EventsLoaded.GetCurrentValue());
        }


        /// <summary>
        /// Gets all new domain events that have been applied to the aggregate root instance since the fetching from the database.
        /// </summary>
        /// <returns>A collection of domain events.</returns>
        public virtual IEnumerable<IDomainEvent> GetChangesEventStream()
        {
            return _changes;
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
                _changes.Add(e);
                listOfMessages.AddRange(await ApplyOneEvent((dynamic)e, new Internal()));
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
                _changes.Add(e);
                listOfMessages.AddRange(await ApplyOneEvent((dynamic)e, new Internal()));
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
        /// <param name="nothing">This parameter is only here to block any acces to this method from outside of the library</param>
        public async Task<IEnumerable<IMessaging>> ApplyOneEvent<TEvent>(TEvent ev, Internal nothing)
            where TEvent : IDomainEvent
        {
            eventStream.Add(ev);
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

    public class Internal
    {
        internal Internal() { }
    }
}
