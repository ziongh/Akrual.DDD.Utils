using System.Collections.Generic;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents.SpecialEvents;

namespace Akrual.DDD.Utils.Domain.EventStorage
{
    public interface IEventStream
    {
        string StreamName { get; set; }
        Task<IEnumerable<IRecordedEvent>> Events { get; set; }
    }
}