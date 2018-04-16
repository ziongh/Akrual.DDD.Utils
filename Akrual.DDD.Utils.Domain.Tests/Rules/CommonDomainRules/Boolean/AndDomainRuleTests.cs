using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Akrual.DDD.Utils.Internal.Tests;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Rules.CommonDomainRules.Boolean
{
    public class AndDomainRuleTests : BaseTests
    {
        [Theory]
        [InlineData(true,true,true)]
        [InlineData(true,false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public async Task Evaluate_TrueAndTrue_ReturnsTrue(bool left, bool right, bool expected)
        {
            var leftRule =
                left
                    ? (BoolenaDomainRule<ExampleAggregate>) new TrueDomainRule<ExampleAggregate>()
                    : new FalseDomainRule<ExampleAggregate>();

            var rightRule =
                right
                    ? (BoolenaDomainRule<ExampleAggregate>)new TrueDomainRule<ExampleAggregate>()
                    : new FalseDomainRule<ExampleAggregate>();

            var andRule = new AndDomainRule<ExampleAggregate>(leftRule, rightRule);
            var exampleEntityFactory = new FactoryWithDefaultObjectCreation();
            var entity = await exampleEntityFactory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid());

            var evaluatedValue = andRule.EvaluateRules(entity);

            Assert.Equal(expected, evaluatedValue);
        }

    }
}
