using Akrual.DDD.Utils.Domain.Aggregates;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainCommands
{
    /// <summary>Represents a command context for aggregate command handler handling command.
    /// </summary>
    public interface ICommandContext
    {
        /// <summary>Add a new aggregate into the current command context.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        void Add<T,TInside>(T aggregateRoot) where T : AggregateRoot<TInside>;

        /// <summary>Get an aggregate from the current command context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TInside"></typeparam>
        /// <param name="id"></param>
        /// <param name="firstFromCache"></param>
        /// <returns></returns>
        T Get<T, TInside>(object id, bool firstFromCache = true) where T : AggregateRoot<TInside>;
        /// <summary>Set the command handle result.
        /// </summary>
        /// <param name="result"></param>
        void SetResult(string result);
        /// <summary>Get the command handle result.
        /// </summary>
        /// <returns></returns>
        string GetResult();
    }
}
