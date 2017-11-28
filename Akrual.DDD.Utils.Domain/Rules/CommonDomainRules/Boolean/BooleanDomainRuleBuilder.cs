using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean
{
    /// <summary>
    /// A builder to create Complex DomainRules based on Boolean Expressions. It can Wrap rules in AND, OR, XOR statements.
    /// </summary>
    /// <typeparam name="TEntity">Any Entity object</typeparam>
    public class BooleanDomainRuleBuilder<TEntity>
        where TEntity : IEntity
    {
        private bool _isDefault;
        private BoolenaDomainRule<TEntity> _result;

        /// <summary>
        /// creates a builder to create Complex DomainRules based on Boolean Expressions. It can Wrap rules in AND, OR, XOR statements.
        /// It receives an initial rule. If none is provided, it will start with a Always true rule.
        /// </summary>
        /// <param name="initial"></param>
        public BooleanDomainRuleBuilder(BoolenaDomainRule<TEntity> initial = null)
        {
            if (initial == null)
            {
                _isDefault = true;
                initial = new TrueDomainRule<TEntity>();
            }

            _result = initial;
        }

        /// <summary>
        /// Start the Fluent Creation Proccess
        /// </summary>
        /// <returns>Fluent object to create</returns>
        public BoolenaDomainRule<TEntity> Create()
        {
            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initial"></param>
        /// <returns></returns>
        public BooleanDomainRuleBuilder<TEntity> And(BoolenaDomainRule<TEntity> initial)
        {
            _result = new AndDomainRule<TEntity>(_result, initial);
            return this;
        }

        public BooleanDomainRuleBuilder<TEntity> Or(BoolenaDomainRule<TEntity> initial)
        {
            if (_isDefault && _result is TrueDomainRule<TEntity>)
            {
                _result = new FalseDomainRule<TEntity>();
                _isDefault = false;
            }

            _result = new OrDomainRule<TEntity>(_result, initial);
            return this;
        }

        public BooleanDomainRuleBuilder<TEntity> Xor(BoolenaDomainRule<TEntity> initial)
        {
            if (_isDefault && _result is TrueDomainRule<TEntity>)
            {
                _result = new FalseDomainRule<TEntity>();
                _isDefault = false;
            }

            _result = new XorDomainRule<TEntity>(_result, initial);
            return this;
        }
    }
}