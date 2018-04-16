using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Akrual.DDD.Utils.Internal.Tests;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Rules.CommonDomainRules.Boolean
{
    public class BooleanDomainRuleBuilderTests : BaseTests
    {
        [Fact]
        public void Create_EmptyContructor_ReturnsTrueRule()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>();
            var trueRule = ruleContructor.Create();

            Assert.IsType<TrueDomainRule<ExampleAggregate>>(trueRule);
        }

        [Fact]
        public void Create_WithContructorNameNotEmpty_ReturnsNameNotEmptyRule()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>(new NameIsNotEmptyRule());
            var nameRule = ruleContructor.Create();

            Assert.IsNotType<TrueDomainRule<ExampleAggregate>>(nameRule);
            Assert.IsType<NameIsNotEmptyRule>(nameRule);
        }

        [Fact]
        public void AND_WithEmptyContructorAndPassingNameNotEmpty_ReturnsAndRuleWithBothInsideIt()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>().And(new NameIsNotEmptyRule());
            var andRule = ruleContructor.Create();

            Assert.IsNotType<TrueDomainRule<ExampleAggregate>>(andRule);
            Assert.IsNotType<NameIsNotEmptyRule>(andRule);
            Assert.IsType<AndDomainRule<ExampleAggregate>>(andRule);
        }

        [Fact]
        public void OR_WithEmptyContructorAndPassingNameNotEmpty_ReturnsORRuleWithBothInsideIt()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>().Or(new NameIsNotEmptyRule());
            var andRule = ruleContructor.Create();

            Assert.IsNotType<TrueDomainRule<ExampleAggregate>>(andRule);
            Assert.IsNotType<NameIsNotEmptyRule>(andRule);
            Assert.IsType<OrDomainRule<ExampleAggregate>>(andRule);
        }

        [Fact]
        public void XOR_WithEmptyContructorAndPassingNameNotEmpty_ReturnsXORRuleWithBothInsideIt()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>().Xor(new NameIsNotEmptyRule());
            var andRule = ruleContructor.Create();

            Assert.IsNotType<TrueDomainRule<ExampleAggregate>>(andRule);
            Assert.IsNotType<NameIsNotEmptyRule>(andRule);
            Assert.IsType<XorDomainRule<ExampleAggregate>>(andRule);
        }


        [Fact]
        public async Task XOR_DefaultBuilderWithNameRulePassingNonEmptyName_ReturnsTrue()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>().Xor(new NameIsNotEmptyRule());
            var andRule = ruleContructor.Create();

            var exampleEntityFactory = new FactoryWithDefaultObjectCreation();
            var entity = await exampleEntityFactory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid()); // It has name

            var evaluatedValue = andRule.EvaluateRules(entity);

            Assert.True(evaluatedValue);
        }

        [Fact]
        public async Task XOR_DefaultBuilderWithNameRulePassingEmptyName_ReturnsFalse()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>().Xor(new NameIsNotEmptyRule());
            var andRule = ruleContructor.Create();

            var exampleEntityFactory = new FactoryWithDefaultObjectCreation();
            exampleEntityFactory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName(null);
            var entity = await exampleEntityFactory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid()); // name is null

            var evaluatedValue = andRule.EvaluateRules(entity);

            Assert.False(evaluatedValue);
        }

        [Fact]
        public async Task OR_DefaultBuilderWithNameRulePassingNonEmptyName_ReturnsTrue()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>().Or(new NameIsNotEmptyRule());
            var andRule = ruleContructor.Create();

            var exampleEntityFactory = new FactoryWithDefaultObjectCreation();
            var entity = await exampleEntityFactory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid()); // It has name

            var evaluatedValue = andRule.EvaluateRules(entity);

            Assert.True(evaluatedValue);
        }

        [Fact]
        public async Task OR_DefaultBuilderWithNameRulePassingEmptyName_ReturnsFalse()
        {
            var ruleContructor = new BooleanDomainRuleBuilder<ExampleAggregate>().Or(new NameIsNotEmptyRule());
            var andRule = ruleContructor.Create();

            var exampleEntityFactory = new FactoryWithDefaultObjectCreation();
            exampleEntityFactory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName(null);
            var entity = await exampleEntityFactory.CreateAsOf(GuidGenerator.GenerateTimeBasedGuid()); // name is null

            var evaluatedValue = andRule.EvaluateRules(entity);

            Assert.False(evaluatedValue);
        }

        protected internal class NameIsNotEmptyRule : BoolenaDomainRule<ExampleAggregate>
        {
            public override bool EvaluateRules(ExampleAggregate entity)
            {
                return !String.IsNullOrEmpty(entity.Name);
            }
        }
    }
}
