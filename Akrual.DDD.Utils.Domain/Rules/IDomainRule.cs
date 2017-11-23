using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules
{
    internal interface IDomainRule<in TEntity, out TRuleReturn> 
        where TEntity : IEntity
    {
        bool ForceFinishExecutor { get; set; }
        float Order { get; set; }
        TRuleReturn EvaluateRules(TEntity entity);
    }
}