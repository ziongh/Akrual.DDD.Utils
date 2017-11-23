using System;
using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean
{
    /// <summary>
    /// This is a Dummy Rule used to start some Complex boolean rule.
    /// </summary>
    /// <typeparam name="TEntity">Any Entity object</typeparam>
    public class TrueDomainRule<TEntity> : BoolenaDomainRule<TEntity>
        where TEntity : IEntity
    {
        /// <inheritdoc />
        public override bool EvaluateRules(TEntity entity)
        {
            return true;
        }
    }
}