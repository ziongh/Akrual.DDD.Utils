using System;
using System.Collections.Generic;
using System.Linq;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Xunit;

namespace Akrual.DDD.Domain.Tests.Utils
{
    /// <summary>
    /// Provides infrastructure for a set of tests on a given aggregate.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class BaseAggregateRootTests<TAggregate, T>
        where TAggregate : AggregateRoot<T>
    {
        private TAggregate sut;

        protected void Test(TAggregate initial, IEnumerable<IDomainEvent> given, Func<TAggregate, Func<IDomainEvent[]>> when, Action<Func<IDomainEvent[]>> then)
        {
            sut = initial;
            then(when(ApplyEvents(sut, given)));
        }

        protected IEnumerable<IDomainEvent> Given(params IDomainEvent[] events)
        {
            return events;
        }

        protected Func<TAggregate, Func<IDomainEvent[]>> When<TCommand>(TCommand command)
            where TCommand : IDomainCommand
        {
            return agg => (() => DispatchCommand(command).Cast<IDomainEvent>().ToArray());
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
                    if (e is TException)
                        Assert.True(true); // Got correct exception type
                    else if (e is CommandHandlerNotDefiendException)
                        Assert.True(false, (e as Exception).Message);
                    else
                        Assert.True(false, string.Format(
                            "Expected exception {0}, but got exception {1}",
                            typeof(TException).Name, e.GetType().Name));
                }
            };
        }

        private IEnumerable<IDomainEvent> DispatchCommand<TCommand>(TCommand c)
            where TCommand : IDomainCommand
        {
            if (c != null)
            {
                
            }

            var handler = sut as IHandleDomainCommand<TCommand>;
            if (handler == null)
                throw new CommandHandlerNotDefiendException(string.Format(
                    "Aggregate {0} does not yet handle command {1}",
                    sut.GetType().Name, c.GetType().Name));
            return handler.Handle(null,c);
        }

        private TAggregate ApplyEvents(TAggregate agg, IEnumerable<IDomainEvent> events)
        {
            if(events != null && events.Any())
                agg.ApplyEvents(events);
            return agg;
        }

        private class CommandHandlerNotDefiendException : Exception
        {
            public CommandHandlerNotDefiendException(string msg) : base(msg) { }
        }
    }
}
