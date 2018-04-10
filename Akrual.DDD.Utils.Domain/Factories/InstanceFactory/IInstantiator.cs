using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories.InstanceFactory
{
    /// <summary>
    /// This is a dangerous Interface. Because it creates one Async Scope just to create a new instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInstantiator<out T>  where T : class
    {
        T Create();
    }
}