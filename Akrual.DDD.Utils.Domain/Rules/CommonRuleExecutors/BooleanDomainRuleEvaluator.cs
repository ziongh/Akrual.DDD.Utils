using System.Collections.Generic;
using System.Linq;
using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules.CommonRuleExecutors
{
    public class BooleanDomainRuleEvaluator<TEntity> : IDomainRuleEvaluator<TEntity, bool>
        where TEntity : IEntity
    {
        internal List<IDomainRule<TEntity, bool>> _rules = new List<IDomainRule<TEntity, bool>>();

        public virtual bool ExecuteAllRules(TEntity customer)
        {
            var result = true;

            foreach (var rule in _rules.OrderBy(s => s.Order))
            {
                result &= rule.EvaluateRules(customer);
                if (rule.ForceFinishExecutor) break;
            }
            return result;
        }
    }
}
