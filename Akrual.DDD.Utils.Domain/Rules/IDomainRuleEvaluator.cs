using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules
{
    internal interface IDomainRuleEvaluator<in TEntity, out TRuleReturn>
        where TEntity : IEntity
    {
        TRuleReturn ExecuteAllRules(TEntity customer);
    }
}