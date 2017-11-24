using System;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomain;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Factories
{
    public class FactoryTests
    {
        [Fact]
        public void Create_FactoryWithOnObjectCreatingSettingName_NameShouldBeSet()
        {
            var factory = new FactoryWithOnObjectCreating();
            var exampleAggregate = factory.Create();
            Assert.Equal("AnotherName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

        [Fact]
        public void Create_FactoryWithDefaultObjectCreationSettingName_NameShouldBeSet()
        {
            var factory = new FactoryWithDefaultObjectCreation();
            var exampleAggregate = factory.Create();
            Assert.Equal("OneName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }
    }



    internal class FactoryWithOnObjectCreating : FactoryWithDefaultObjectCreation
    {
        private Factory<ExampleAggregate, ExampleAggregate> _factoryImplementation;

        public FactoryWithOnObjectCreating()
        {
            OnAggregateCreation += SetNameToLala;
        }

        private void SetNameToLala(object sender, FactoryCreationExecutingContext<ExampleAggregate, ExampleAggregate> context)
        {
            context.ObjectBeingCreated.Name = "AnotherName";
        }
    }
}
