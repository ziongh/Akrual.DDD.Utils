using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomain;
using Akrual.DDD.Utils.Internal.Tests;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Rules.CommonDomainRules.Boolean
{
    public class OrDomainRuleTests : BaseTests
    {
        [Theory]
        [InlineData(true,true,true)]
        [InlineData(true,false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void Evaluate_TrueAndTrue_ReturnsTrue(bool left, bool right, bool expected)
        {
            var leftRule =
                left
                    ? (BoolenaDomainRule<ExampleAggregate>) new TrueDomainRule<ExampleAggregate>()
                    : new FalseDomainRule<ExampleAggregate>();

            var rightRule =
                right
                    ? (BoolenaDomainRule<ExampleAggregate>)new TrueDomainRule<ExampleAggregate>()
                    : new FalseDomainRule<ExampleAggregate>();

            var andRule = new OrDomainRule<ExampleAggregate>(leftRule, rightRule);
            var exampleEntityFactory = new FactoryWithDefaultObjectCreation();
            var entity = exampleEntityFactory.Create();

            var evaluatedValue = andRule.EvaluateRules(entity);

            Assert.Equal(expected, evaluatedValue);
        }

    }
}
