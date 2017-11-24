﻿using System;
using Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomain;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Rules
{
    public class BooleanDomainRuleBuilderTests
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

        protected internal class NameIsNotEmptyRule : BoolenaDomainRule<ExampleAggregate>
        {
            public override bool EvaluateRules(ExampleAggregate entity)
            {
                return !String.IsNullOrEmpty(entity.Name);
            }
        }
    }
}
