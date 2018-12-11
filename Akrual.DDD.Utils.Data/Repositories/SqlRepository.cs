using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.DbContexts;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Akrual.DDD.Utils.Data.Repositories
{
    public class SqlRepository<T> : IRepository<T> where T : class, IAggregateRoot
    {
        private readonly IFactory<T> _factory;
        private readonly IAkrualContext _context;

        public SqlRepository(IFactory<T> factory,IAkrualContext context)
        {
            _factory = factory;
            _context = context;
        }

        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order. Then it check all later events that should be applied before the given
        ///     point and applies them too.
        ///     <remarks> 
        ///         In other words, it will give the result of a particular time point within the stream (everything we know 
        ///         that happened until that point in time, counting with events that may have happened later, but affects some point back in time) 
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public async Task<T> CreateAsOf(Guid guid, DateTime? AsOfDate = null)
        {
            var entry = await _context.FindBy<T>(s => s.Id == guid).FirstOrDefaultAsync();
            entry = entry ?? await _factory.Create(guid);
            return entry;
        }

        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order.
        ///     <remarks> 
        ///         In other words, it will give the result at a particular time point within the stream (what did we know as at this point in time) 
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        public async Task<T> CreateAsAt(Guid guid, DateTime? AsAtDate = null)
        {
            return await _context.FindBy<T>(s => s.Id == guid).FirstOrDefaultAsync();
        }
    }
}
