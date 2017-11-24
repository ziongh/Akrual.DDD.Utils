using System;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomain
{
    public class ExampleAggregate : AggregateRoot<ExampleAggregate>
    {
        public ExampleAggregate(Guid id) : base(id)
        {
        }

        public string Name { get; internal set; }
        public int Number { get; internal set; }
        public DateTime Date { get; internal set; }
    }
}