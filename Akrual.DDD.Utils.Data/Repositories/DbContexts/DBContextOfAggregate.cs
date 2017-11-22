using Microsoft.EntityFrameworkCore;

namespace Akrual.DDD.Utils.Data.Repositories.DbContexts
{
    public abstract class DBContextOfAggregate : DbContext
    {
        protected DBContextOfAggregate(DbContextOptions<DBContextOfAggregate> options) : base(options)
        {
        }
    }
}
