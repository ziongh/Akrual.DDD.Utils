using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Akrual.DDD.Utils.Internal.Tests;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Factories
{
    public class FactoryTests : BaseTests
    {
        [Fact]
        public async Task Create_FactoryWithDefaultObjectCreationSettingName_NameShouldBeSet()
        {
            var factory = new FactoryWithDefaultObjectCreation();
            var exampleAggregate = await factory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid());
            Assert.Equal("OneName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }
        

        [Fact]
        public async Task Create_FactoryWithOnObjectCreatingSettingName_NameShouldBeSet()
        {
            var factory = new FactoryWithOnObjectCreating();
            var exampleAggregate = await factory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid());
            Assert.Equal("AnotherName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

        [Fact]
        public async Task Create_FactoryWithOnObjectCreatingSettingNameTwice_NameShouldBeSetTwice()
        {
            var factory = new FactoryWithOnObjectCreating();
            factory.OnAfterCreateDefaultInstance += factory.SetNameToYetAnotherName;
            var exampleAggregate = await factory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid());

            Assert.Equal("YetAnotherName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

        [Fact]
        public async Task Create_FactoryWithOnObjectCreatingSettingNameTwicethenDeleteOne_NameShouldBeSetOnce()
        {
            var factory = new FactoryWithOnObjectCreating();
            factory.OnAfterCreateDefaultInstance += factory.SetNameToYetAnotherName;
            factory.OnAfterCreateDefaultInstance -= factory.SetNameToYetAnotherName;
            var exampleAggregate = await factory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid());

            Assert.Equal("AnotherName", exampleAggregate.Name);
            Assert.NotEqual(Guid.Empty, exampleAggregate.Id);
        }

    }
    

    internal class FactoryWithOnObjectCreating : FactoryWithDefaultObjectCreation
    {
        public FactoryWithOnObjectCreating() : base()
        {
            OnAfterCreateDefaultInstance += this.SetNameToAnotherName;
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
