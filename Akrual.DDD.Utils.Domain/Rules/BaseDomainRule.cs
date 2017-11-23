using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules
{
    /// <summary>
    /// Base class for Rule Definition. It should receive one Entity as input (TEntity) and return a value as Output (TRuleReturn). This output will be used in the RuleExecutor.
    /// </summary>
    /// <typeparam name="TEntity">The type of the Input</typeparam>
    /// <typeparam name="TRuleReturn">The type of the Output</typeparam>
    public abstract class BaseDomainRule<TEntity, TRuleReturn> : IDomainRule<TEntity, TRuleReturn>
        where TEntity : IEntity
    {
        public bool ForceFinishExecutor { get; set; }
        public float Order { get; set; }

        /// <summary>
        /// Evaluate all Rules to a specific entity and and return output
        /// </summary>
        /// <param name="entity">Any entity object that will be analysed</param>
        /// <returns>Evaluated value</returns>
        public abstract TRuleReturn EvaluateRules(TEntity entity);
    }
}