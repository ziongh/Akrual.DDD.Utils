using System;
using System.Collections.Generic;
using System.Text;
using EntityFrameworkCoreMock;

namespace Akrual.DDD.Utils.Internal.Extensions
{
    public static class DbAsyncEnumerableExtensions
    {
        public static DbAsyncEnumerable<T> CreateDbAsyncEnumerable<T>()
        {
            return new DbAsyncEnumerable<T>(new List<T>());
        }
    }
}
