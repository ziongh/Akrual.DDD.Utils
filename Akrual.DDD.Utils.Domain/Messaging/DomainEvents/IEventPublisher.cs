using MediatR;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents
{
    /// <summary>
    /// Interface 
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventPublisher<in TEvent> : INotificationHandler<TEvent> where TEvent : DomainEvent
    {
        
    }
}