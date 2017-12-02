using System;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Internal.Contracts;
using Akrual.DDD.Utils.Internal.Extensions;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomain
{
    public class ExampleAggregate : AggregateRoot<ExampleAggregate>
    {
        public ExampleAggregate(Guid id, string name, int number, DateTime date) : base(id)
        {
            Name = name;
            Number = number;
            Date = date;
        }

        public string Name { get; private set; }
        public int Number { get; private set; }
        public DateTime Date { get; private set; }

        public void FixName(string newName)
        {
            CommonContract.EnsuresNotNull(newName);
            Name = newName;
        }
    }
    
}
