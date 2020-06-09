using System;
using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean
{
    /// <summary>
    /// This Rule is used to wrap two Boolean rules using an AND expression.
    /// </summary>
    /// <typeparam name="TEntity">Any Entity object</typeparam>
    public class AndDomainRule<TEntity> : BoolenaDomainRule<TEntity>
        where TEntity : IEntity
    {
        private readonly BoolenaDomainRule<TEntity> _left;
        private readonly BoolenaDomainRule<TEntity> _right;

        /// <summary>
        /// Creates a Rule that is used to wrap two Boolean rules using an AND expression.
        /// </summary>
        /// <param name="left">Left side of the Boolean Expression</param>
        /// <param name="right">Right side of the Boolean Expression</param>
        public AndDomainRule(BoolenaDomainRule<TEntity> left, BoolenaDomainRule<TEntity> right)
        {
            _left = left ?? throw new ArgumentNullException("Left rule must be informed");
            _right = right ?? throw new ArgumentNullException("Right rule must be informed");
        }

        /// <inheritdoc />
        public override bool EvaluateRules(TEntity entity)
        {
            return _left.EvaluateRules(entity) && _right.EvaluateRules(entity);
        }
    }
}