using System;

namespace Akrual.DDD.Utils.Domain.Entities
{
    public interface IEntity<T>
    {
        Guid Id { get; }
    }
}
