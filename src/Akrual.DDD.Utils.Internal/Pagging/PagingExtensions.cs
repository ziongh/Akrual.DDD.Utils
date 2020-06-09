using System.Collections.Generic;
using System.Linq;

namespace Akrual.DDD.Utils.Internal.Pagging
{
    public static class PagingExtensions
    {
        public static Paged<T> Page<T>(this IEnumerable<T> collection, PageInfo paging)
        {
            paging = paging ?? new PageInfo();

            return new Paged<T>
            {
                Items = collection.Skip(paging.PageIndex * paging.PageSize).Take(paging.PageSize).AsQueryable(),
                Paging = paging,
            };
        }

        public static Paged<T> Page<T>(this IQueryable<T> collection, PageInfo paging)
        {
            paging = paging ?? new PageInfo();

            return new Paged<T>
            {
                Items = collection.Skip(paging.PageIndex * paging.PageSize).Take(paging.PageSize),
                Paging = paging,
            };
        }
    }
}
