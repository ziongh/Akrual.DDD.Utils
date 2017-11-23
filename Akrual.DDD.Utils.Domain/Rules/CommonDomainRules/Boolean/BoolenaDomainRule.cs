using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules.CommonDomainRules.Boolean
{
    /// <summary>
    /// This is a Base Class to define any rules that return a Boolean.
    /// </summary>
    /// <typeparam name="TEntity">Any Entity object</typeparam>
    public abstract class BoolenaDomainRule<TEntity> : BaseDomainRule<TEntity, bool>
        where TEntity : IEntity
    {
      
    }
}
