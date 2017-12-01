using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.DomainCommands;
using Akrual.DDD.Utils.Domain.DomainEvents;
using Xunit;

namespace Akrual.DDD.Domain.Tests.Utils
{
    /// <summary>
    /// Provides infrastructure for a set of tests on a given aggregate.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="T"></typeparam>
    class BaseAggregateRootTests<TAggregate, T>
        where TAggregate : AggregateRoot<T>
    {
        private TAggregate sut;

        protected void Test(IEnumerable<DomainEvent> given, Func<TAggregate, object> when, Action<object> then)
        {
            then(when(ApplyEvents(sut, given)));
        }

        protected IEnumerable<DomainEvent> Given(params DomainEvent[] events)
        {
            return events;
        }

        protected Func<TAggregate, DomainEvent[]> When<TCommand>(TCommand command)
            where TCommand : IDomainCommand
        {
            return agg => DispatchCommand(command).Cast<DomainEvent>().ToArray();
        }

        protected Action<object> Then(params object[] expectedEvents)
        {
            return got =>
            {
                var gotEvents = got as object[];
                if (gotEvents != null)
                {
                    if (gotEvents.Length == expectedEvents.Length)
                        for (var i = 0; i < gotEvents.Length; i++)
                            if (gotEvents[i].GetType() == expectedEvents[i].GetType())
                                Assert.Equal(Serialize(expectedEvents[i]), Serialize(gotEvents[i]));
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
                else if (got is CommandHandlerNotDefiendException)
                    Assert.True(false, (got as Exception).Message);
                else
                    Assert.True(false, string.Format("Expected events, but got exception {0}",
                        got.GetType().Name));
            };
        }

        private string[] EventDiff(object[] a, object[] b)
        {
            var diff = a.Select(e => e.GetType().Name).ToList();
            foreach (var remove in b.Select(e => e.GetType().Name))
                diff.Remove(remove);
            return diff.ToArray();
        }

        protected Action<object> ThenFailWith<TException>()
        {
            return got =>
            {
                if (got is TException)
                    Assert.True(true); // Got correct exception type
                else if (got is CommandHandlerNotDefiendException)
                    Assert.True(false,(got as Exception).Message);
                else if (got is Exception)
                    Assert.True(false,string.Format(
                        "Expected exception {0}, but got exception {1}",
                        typeof(TException).Name, got.GetType().Name));
                else
                    Assert.True(false,string.Format(
                        "Expected exception {0}, but got event result",
                        typeof(TException).Name));
            };
        }

        private IEnumerable<DomainEvent> DispatchCommand<TCommand>(TCommand c)
            where TCommand : IDomainCommand
        {
            var handler = sut as IHandleDomainCommand<TCommand>;
            if (handler == null)
                throw new CommandHandlerNotDefiendException(string.Format(
                    "Aggregate {0} does not yet handle command {1}",
                    sut.GetType().Name, c.GetType().Name));
            return handler.Handle(null,c);
        }

        private TAggregate ApplyEvents(TAggregate agg, IEnumerable<DomainEvent> events)
        {
            agg.ApplyEvent(events);
            return agg;
        }

        private string Serialize(object obj)
        {
            var ser = new XmlSerializer(obj.GetType());
            var ms = new MemoryStream();
            ser.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return new StreamReader(ms).ReadToEnd();
        }

        private class CommandHandlerNotDefiendException : Exception
        {
            public CommandHandlerNotDefiendException(string msg) : base(msg) { }
        }
    }
}
