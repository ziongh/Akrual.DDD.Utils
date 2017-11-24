using System;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Utils.UUID;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomain
{
    internal class FactoryWithDefaultObjectCreation : Factory<ExampleAggregate, ExampleAggregate>
    {
        private Factory<ExampleAggregate, ExampleAggregate> _factoryImplementation;

        protected override ExampleAggregate CreateDefaultInstance()
        {
            return new ExampleAggregate(GuidGenerator.GenerateTimeBasedGuid()) { Name = "OneName", Date = new DateTime(1990,5,12), Number = 100};
        }
    }
}