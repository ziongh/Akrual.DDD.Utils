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
        public void Create_Test_return_ok()
        {
            var factory = new MyFactory();
            var final = factory.Create();
            Assert.Equal("lala", final.Name);
            Assert.NotEqual(Guid.Empty,final.Id);
        }


        [Fact]
        public void Create_Test_return_ok2()
        {
            var factory = new MyFactory();
            var final = factory.Create();

            Assert.True(final.IsValid);
        }


        public class MyAggregate : AggregateRoot<MyAggregate>
        {
            public MyAggregate(Guid id) : base(id)
            {
            }

            public string Name { get; internal set; }
        }

        public class MyFactory : Factory<MyAggregate, MyAggregate>
        {
            private Factory<MyAggregate, MyAggregate> _factoryImplementation;

            public MyFactory()
            {
                OnAggregateCreation += SetNameToLala;
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
