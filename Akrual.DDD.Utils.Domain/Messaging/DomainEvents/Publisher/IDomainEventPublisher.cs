using System.Threading;
using System.Threading.Tasks;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents.Publisher
{
    public interface IDomainEventPublisher
    {
        Task Publish<Tevent>(Tevent request,
            CancellationToken cancellationToken) where Tevent : IDomainEvent;
    }
}
