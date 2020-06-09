using System;

namespace Akrual.DDD.Utils.Domain.Entities
{
    public interface IEntity
    {
        Guid Id { get; }
    }
}
