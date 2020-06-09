using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules
{
    /// <summary>
    /// Base interface for Rule Definition. It should receive one Entity as input (TEntity) and return a value as Output (TRuleReturn). This output will be used in the RuleExecutor.
    /// </summary>
    /// <typeparam name="TEntity">The type of the Input</typeparam>
    /// <typeparam name="TRuleReturn">The type of the Output</typeparam>
    public interface IDomainRule<in TEntity, out TRuleReturn> 
        where TEntity : IEntity
    {
        bool ForceFinishExecutor { get; set; }
        float Order { get; set; }
        TRuleReturn EvaluateRules(TEntity entity);
    }
}