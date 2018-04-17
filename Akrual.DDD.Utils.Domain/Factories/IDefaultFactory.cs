﻿using System;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Factories
{
    /// <summary>
    /// This interface represents a default implementation of an Factory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDefaultFactory<T> where T : IAggregateRoot
    {
        event EventHandler<FactoryCreationExecutingContext<T, T>> OnAfterCreateDefaultInstance;

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
        Task<T> CreateAsOf(Guid guid, DateTime? AsOfDate = null);
        
        /// <summary>
        ///     Creates The aggregate, then query the event store for all it's events until a certain point. 
        ///     And applies them in order.
        ///     <remarks> 
        ///         In other words, it will give the result at a particular time point within the stream (what did we know as at this point in time) 
        ///     </remarks>
        /// </summary>
        /// <returns>Returns the Filled Aggregate with all the invariants Checked.</returns>
        Task<T> CreateAsAt(Guid guid, DateTime? AsAtDate = null);
    }
}