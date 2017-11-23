using System;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests
{
    public class FactoryTests
    {
        [Fact]
        public void test()
        {
            var factory = new MyFactory();
            var final = factory.Create();
            Assert.Equal("lala", final.Name);
            Assert.NotEqual(Guid.Empty,final.Id);
        }


        internal class MyAggregate : AggregateRoot<MyAggregate>
        {
            public MyAggregate(Guid id) : base(id)
            {
            }

            internal string Name { get; set; }
        }

        internal class MyFactory : Factory<MyAggregate, MyAggregate>
        {
            private Factory<MyAggregate, MyAggregate> _factoryImplementation;

            public MyFactory()
            {
                AggregateCreation += SetNameToLala;
            }

            private void SetNameToLala(object sender, FactoryCreationExecutingContext<MyAggregate, MyAggregate> context)
            {
                context.ObjectBeingCreated.Name = "lala";
            }

            protected override MyAggregate CreateDefaultInstance()
            {
                return new MyAggregate(GuidGenerator.GenerateTimeBasedGuid());
            }
        }
    }
}
