using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules
{
    /// <summary>
    /// Represents a rule Engine Executor. It will Crunch all the Rules associated to it and return the final output.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRuleReturn"></typeparam>
    public abstract class BaseDomainRuleEvaluator<TEntity, TRuleReturn> : IDomainRuleEvaluator<TEntity, TRuleReturn>
        where TEntity : IEntity
    {
        internal List<IDomainRule<TEntity, TRuleReturn>> _rules = new List<IDomainRule<TEntity, TRuleReturn>>();

        public abstract TRuleReturn ExecuteAllRules(TEntity customer);
    }
}