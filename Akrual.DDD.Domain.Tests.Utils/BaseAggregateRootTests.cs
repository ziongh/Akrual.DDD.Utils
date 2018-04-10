using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace Akrual.DDD.Domain.Tests.Utils
{
    /// <summary>
    /// Provides infrastructure for a set of tests on a given aggregate.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseAggregateRootTests<TAggregate, T> : IDisposable
        where TAggregate : AggregateRoot<T>
        where T : new()
    {
        protected Container _container;
        protected readonly Scope _scope;

        /// <summary>
        /// This method should only be implemented if the test is going to use Dependency Injection.
        /// Otherwise, leave it blank.
        /// </summary>
        public abstract void RegisterAllToContainer();

        protected BaseAggregateRootTests()
        {
            RegisterAllToContainer();
            if (_container != null)
            {
                this._scope = AsyncScopedLifestyle.BeginScope(_container);
            }
        }
        
        public void Dispose()
        {
            _scope?.Dispose();
            _container?.Dispose();
        }

        private TAggregate sut;

        protected async Task Test(TAggregate initial, IEnumerable<IDomainEvent> given, Func<TAggregate, Func<IDomainEvent[]>> when, Action<Func<IDomainEvent[]>> then)
        {
            sut = initial;
            then(when(await ApplyEvents(sut, given)));
        }

        protected IEnumerable<IDomainEvent> Given(params IDomainEvent[] events)
        {
            return events;
        }

        protected Func<TAggregate, Func<IDomainEvent[]>> When<TCommand>(TCommand command)
            where TCommand : IDomainCommand
        {
            return agg => (() =>
            {
                try
                {
                    var result = DispatchCommand(command).Result;
                    return result.ToArray();
                }
                catch (AggregateException e)
                {
                    throw e.InnerExceptions.FirstOrDefault();
                }
            });
        }

        protected Action<Func<IDomainEvent[]>> Then(params IDomainEvent[] expectedEvents)
        {
            return got =>
            {
                var gotEventsFunc = got;
                try
                {
                    var gotEvents = gotEventsFunc();
                    if (gotEvents.Length == expectedEvents.Length)
                        for (var i = 0; i < gotEvents.Length; i++)
                            if (gotEvents[i].GetType() == expectedEvents[i].GetType())
                                Assert.Equal(expectedEvents[i], gotEvents[i]);
                            else
                                Assert.True(false, string.Format(
                                    "Incorrect event in results; expected a {0} but got a {1}",
                                    expectedEvents[i].GetType().Name, gotEvents[i].GetType().Name));
                    else if (gotEvents.Length < expectedEvents.Length)
                        Assert.True(false, string.Format("Expected event(s) missing: {0}",
                            string.Join(", ", EventDiff(expectedEvents, gotEvents))));
                    else
                        Assert.True(false, string.Format("Unexpected event(s) emitted: {0}",
                            string.Join(", ", EventDiff(gotEvents, expectedEvents))));
                }
                catch (Exception e)
                {
                    if (e is CommandHandlerNotDefiendException)
                        Assert.True(false, (e as Exception).Message);
                    else
                        Assert.True(false, string.Format("Expected events, but got exception {0}",
                            e.GetType().Name));
                }
            };
        }

        private string[] EventDiff(IDomainEvent[] a, IDomainEvent[] b)
        {
            var diff = a.Select(e => e.GetType().Name).ToList();
            foreach (var remove in b.Select(e => e.GetType().Name))
                diff.Remove(remove);
            return diff.ToArray();
        }

        protected Action<Func<IDomainEvent[]>> ThenFailWith<TException>()
            where TException : DomainException
        {
            return got =>
            {
                var gotEventsFunc = got;
                try
                {
                    var gotEvents = gotEventsFunc();
                    Assert.True(false, string.Format(
                        "Expected exception {0}, but got event result",
                        typeof(TException).Name));
                }
                catch (Exception e)
                {
                    var temp = e;
                    if (temp is TException)
                        Assert.True(true); // Got correct exception type
                    else if (temp is CommandHandlerNotDefiendException)
                        Assert.True(false, (temp as Exception).Message);
                    else
                        Assert.True(false, string.Format(
                            "Expected exception {0}, but got exception {1}",
                            typeof(TException).Name, temp.GetType().Name));
                }
            };
        }

        private async Task<IEnumerable<IDomainEvent>> DispatchCommand<TCommand>(TCommand c)
            where TCommand : IDomainCommand
        {
            if (c != null)
            {
                var handler = sut as IHandleDomainCommand<TCommand>;
                if (handler == null)
                    throw new CommandHandlerNotDefiendException(string.Format(
                        "Aggregate {0} does not yet handle command {1}",
                        sut.GetType().Name, c.GetType().Name));
                var messages = await handler.Handle(c,CancellationToken.None);

                // Get All Events that this Command handler has emitted
                var events = messages.OfType<IDomainEvent>().ToList();

                // Get All Commands that this Command handler has emitted
                var commands = messages.OfType<IDomainCommand>().ToList();

                // Foreach Command emitted, Handle them recursively
                foreach (var command in commands)
                {
                    IEnumerable<IDomainEvent> newEvents = await DispatchCommand((dynamic) command);
                    events.AddRange(newEvents);
                }

                // Return all Events that the Command handler and all its recursions generated.
                return events;
            }
            throw new ArgumentNullException(nameof(c));
        }

        private async Task<TAggregate> ApplyEvents(TAggregate agg, IEnumerable<IDomainEvent> events)
        {
            if (events != null)
            {
                var eventsList = events.ToList();
                if (eventsList.Any())
                {
                    await agg.ApplyEvents(eventsList);
                }
            }

            return agg;
        }

        private class CommandHandlerNotDefiendException : Exception
        {
            public CommandHandlerNotDefiendException(string msg) : base(msg) { }
        }

    }
}
