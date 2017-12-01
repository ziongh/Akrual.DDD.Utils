using System;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomain;
using Akrual.DDD.Utils.Internal;
using Akrual.DDD.Utils.Internal.Tests;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Factories
{
    public class FactoryTests : BaseTests
    {
        [Fact]
        public void Create_FactoryWithDefaultObjectCreationSettingName_NameShouldBeSet()
        {
            var factory = new FactoryWithDefaultObjectCreation();
            var exampleAggregate = factory.Create();
            Assert.Equal("OneName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

        [Fact]
        public void Create_FactoryWithOnObjectCreatingSettingName_NameShouldBeSet()
        {
            var factory = new FactoryWithOnObjectCreating();
            var exampleAggregate = factory.Create();
            Assert.Equal("AnotherName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

        [Fact]
        public void Create_FactoryWithOnObjectCreatingSettingNameTwice_NameShouldBeSetTwice()
        {
            var factory = new FactoryWithOnObjectCreating();
            factory.OnAggregateCreation += factory.SetNameToYetAnotherName;
            var exampleAggregate = factory.Create();

            Assert.Equal("YetAnotherName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

        [Fact]
        public void Create_FactoryWithOnObjectCreatingSettingNameTwicethenDeleteOne_NameShouldBeSetOnce()
        {
            var factory = new FactoryWithOnObjectCreating();
            factory.OnAggregateCreation += factory.SetNameToYetAnotherName;
            factory.OnAggregateCreation -= factory.SetNameToYetAnotherName;
            var exampleAggregate = factory.Create();

            Assert.Equal("AnotherName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

    }



    internal class FactoryWithOnObjectCreating : FactoryWithDefaultObjectCreation
    {
        private Factory<ExampleAggregate, ExampleAggregate> _factoryImplementation;

        public FactoryWithOnObjectCreating()
        {
            OnAggregateCreation += SetNameToAnotherName;
        }

        private void SetNameToAnotherName(object sender, FactoryCreationExecutingContext<ExampleAggregate, ExampleAggregate> context)
        {
            context.ObjectBeingCreated.FixName("AnotherName");
        }


        public void SetNameToYetAnotherName(object sender, FactoryCreationExecutingContext<ExampleAggregate, ExampleAggregate> context)
        {
            context.ObjectBeingCreated.FixName("YetAnotherName");
        }
    }
}
