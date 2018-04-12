using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Rules;
using Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean;
using Akrual.DDD.Utils.Domain.Rules.CommonRuleExecutors;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Akrual.DDD.Utils.Internal.Tests;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Rules
{
    public class BooleanDomainRuleEvaluatorTests : BaseTests
    {
        [Fact]
        public async Task Evaluate_SimpleTrueExpression_ReturnsTrue()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule);
            var factory = new FactoryWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.True(evaluateValue);
        }

        [Fact]
        public async Task Evaluate_TwoSimpleTrueExpression_ReturnsTrue()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var trueRule2 = new TrueDomainRule<ExampleAggregate>();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule, trueRule2);
            var factory = new FactoryWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.True(evaluateValue);
        }

        [Fact]
        public async Task Evaluate_TwoSimpleTrueExpressionInsideList_ReturnsTrue()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var trueRule2 = new TrueDomainRule<ExampleAggregate>();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(new List<IDomainRule<ExampleAggregate, bool>>{trueRule, trueRule2});
            var factory = new FactoryWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.True(evaluateValue);
        }


        [Fact]
        public async Task Evaluate_SimpleTrueExpressionAndNameExistsRule_WhenNameIsNotEmpty_ReturnsTrue()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var nameNotEmptyRule = new NameIsNotEmptyRule();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule, nameNotEmptyRule);
            var factory = new FactoryWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.True(evaluateValue);
        }

        [Fact]
        public async Task Evaluate_SimpleTrueExpressionAndNameExistsRule_WhenNameIsNull_ReturnsFalse()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var nameNotEmptyRule = new NameIsNotEmptyRule();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule, nameNotEmptyRule);
            var factory = new FactoryWithDefaultObjectCreation();
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName(null);

            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.False(evaluateValue);
        }

        [Fact]
        public async Task Evaluate_SimpleTrueExpressionAndNameExistsRule_WhenNameIsEmpty_ReturnsFalse()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var nameNotEmptyRule = new NameIsNotEmptyRule();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule, nameNotEmptyRule);
            var factory = new FactoryWithDefaultObjectCreation();
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName("");

            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.False(evaluateValue);
        }


        [Fact]
        public async Task Add_SimpleTrueExpressionAndNameExistsRule_WhenNameIsEmpty_ReturnsFalse()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var nameNotEmptyRule = new NameIsNotEmptyRule();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule);
            evaluator.AddRule(nameNotEmptyRule);
            var factory = new FactoryWithDefaultObjectCreation();
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName("");
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.False(evaluateValue);
        }

        [Fact]
        public async Task AddList_SimpleTrueExpressionAndNameExistsRule_WhenNameIsEmpty_ReturnsFalse()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var nameNotEmptyRule = new NameIsNotEmptyRule();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule);
            evaluator.AddRule(new List<BoolenaDomainRule<ExampleAggregate>> {nameNotEmptyRule});
            var factory = new FactoryWithDefaultObjectCreation();
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName("");
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.False(evaluateValue);
        }


        [Fact]
        public async Task Remove_SimpleTrueExpressionAndNameExistsRuleThenRemoveNameRule_WhenNameIsEmpty_ReturnsTrue()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var nameNotEmptyRule = new NameIsNotEmptyRule();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule);
            evaluator.AddRule(nameNotEmptyRule);
            evaluator.RemoveRule(nameNotEmptyRule);

            var factory = new FactoryWithDefaultObjectCreation();
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName("");
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.True(evaluateValue);
        }


        [Fact]
        public async Task RemoveList_SimpleTrueExpressionAndNameExistsRuleThenRemoveNameRule_WhenNameIsEmpty_ReturnsTrue()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate>();
            var nameNotEmptyRule = new NameIsNotEmptyRule();
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule);
            evaluator.AddRule(nameNotEmptyRule);
            evaluator.RemoveRule(new List<IDomainRule<ExampleAggregate, bool>>{ nameNotEmptyRule });

            var factory = new FactoryWithDefaultObjectCreation();
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName("");
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.True(evaluateValue);
        }

        [Fact]
        public async Task EvaluateWithSkipEnabled_SimpleTrueExpressionAndNameExistsRuleButNameRuleWontRun_WhenNameIsNull_ReturnsTrue()
        {
            var trueRule = new TrueDomainRule<ExampleAggregate> {Order = 1, ForceFinishExecutor = true};
            var nameNotEmptyRule = new NameIsNotEmptyRule { Order = 2, ForceFinishExecutor = false }; // This wont run
            var evaluator = new BooleanDomainRuleEvaluator<ExampleAggregate>(trueRule, nameNotEmptyRule);
            var factory = new FactoryWithDefaultObjectCreation();
            factory.OnAfterCreateDefaultInstance += (sender, context) => context.ObjectBeingCreated.FixName(null);

            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            var evaluateValue = evaluator.ExecuteAllRules(exampleAggregate);

            Assert.True(evaluateValue);
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
