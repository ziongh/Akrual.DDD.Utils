namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents
{
    /// <summary>
    /// Implemented by an aggregate once for each event type it can apply.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IApplyDomainEvent<in TEvent>
        where TEvent : IDomainEvent
    {
        void Apply(TEvent e);
    }
}
