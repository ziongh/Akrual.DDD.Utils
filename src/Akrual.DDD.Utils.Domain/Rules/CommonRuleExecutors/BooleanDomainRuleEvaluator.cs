using System.Collections.Generic;
using System.Linq;
using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules.CommonRuleExecutors
{
    /// <inheritdoc />
    public class BooleanDomainRuleEvaluator<TEntity> : BaseDomainRuleEvaluator<TEntity, bool>
        where TEntity : IEntity
    {
        /// <inheritdoc />
        public BooleanDomainRuleEvaluator(params IDomainRule<TEntity, bool>[] rules) : base(rules)
        {
        }

        /// <inheritdoc />
        public BooleanDomainRuleEvaluator(IEnumerable<IDomainRule<TEntity, bool>> rules) : base(rules)
        {
        }

        /// <inheritdoc />
        public override bool ExecuteAllRules(TEntity entity)
        {
            var result = true;

            foreach (var rule in _rules.OrderBy(s => s.Order))
            {
                result &= rule.EvaluateRules(entity);
                if (rule.ForceFinishExecutor) break;
            }
            return result;
        }
    }
}
