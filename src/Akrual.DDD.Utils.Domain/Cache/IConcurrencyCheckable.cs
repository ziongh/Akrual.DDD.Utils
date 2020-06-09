using System;
using System.Collections.Generic;
using System.Text;

namespace Akrual.DDD.Utils.Domain.Cache
{
    public interface IConcurrencyCheckable
    {
        Guid ConcurrencyGuid {get;set;}
    }
}
