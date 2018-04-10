using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;

namespace Akrual.DDD.Utils.Domain.Messaging.DomainEvents.Publisher
{
    public interface IDomainEventPublisher
    {
        Task<IEnumerable<IDomainCommand>> Publish<Tevent>(Tevent request,
            CancellationToken cancellationToken) where Tevent : IDomainEvent;
    }
}
