using System;
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

    public class EventStream : IEventStream
    {
        public string StreamName { get; set; }
        public Task<IEnumerable<IRecordedEvent>> Events { get; set; }
    }

    public class EventStreamNameComponents
    {
        public EventStreamNameComponents(Type aggregateType, Guid aggregateGuid, string streamBaseName)
        {
            AggregateType = aggregateType;
            AggregateGuid = aggregateGuid;
            this.StreamBaseName = streamBaseName;
        }

        public Type AggregateType { get; set; }
        public Guid AggregateGuid { get; set; }
        public string StreamBaseName { get; set; }
    }
}