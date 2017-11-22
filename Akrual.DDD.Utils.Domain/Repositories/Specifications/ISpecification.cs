using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Akrual.DDD.Utils.Domain.Repositories.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
    }

}
