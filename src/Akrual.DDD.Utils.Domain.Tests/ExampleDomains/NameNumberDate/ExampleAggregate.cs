using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Messaging;
using Akrual.DDD.Utils.Domain.Messaging.Buses;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using MessagePack;

namespace Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate
{
    [MessagePackObject]
    public class CreateExampleAggregate : DomainCommand{
        
        [Key(6)]
        public string Name { get; set; }
        [Key(7)]
        public int Number { get; set; }
        [Key(8)]
        public DateTime Date { get; set; }


        public CreateExampleAggregate(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return Name;
            yield return Number;
            yield return Date;
        }
    }

    [MessagePackObject]
    public class ExampleAggregateCreated : DomainEvent
    {
        [Key(5)]
        public override string EventName { get; } = "ExampleAggregateCreated";


        [Key(9)]
        public string Name { get; set; }
        [Key(10)]
        public int Number { get; set; }
        [Key(11)]
        public DateTime Date { get; set; }


        public ExampleAggregateCreated(Guid eventId,Guid aggregateRootId) : base(eventId,aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return AggregateRootId;
            yield return Name;
            yield return Number;
            yield return Date;
        }

    }

    [MessagePackObject]
    public class ExampleAggregate : AggregateRoot<ExampleAggregate>,
        IHandleDomainCommand<CreateExampleAggregate>,
        IHandleDomainEvent<ExampleAggregateCreated>

    {
        public override string StreamBaseName => "Example";
        public ExampleAggregate(IBus bus) : base(Guid.Empty,bus)
        {
        }
        [Key(6)]
        public string Name { get; private set; }
        [Key(7)]
        public int Number { get; private set; }
        [Key(8)]
        public DateTime Date { get; private set; }

        public void FixName(string newName)
        {
            Name = newName;
        }

        public async Task<IEnumerable<IMessaging>> Handle(CreateExampleAggregate request, CancellationToken cancellationToken)
        {
            // check if command ok
            
            // Emit event
            return GenerateEvents(request);
        }

        private IEnumerable<IDomainEvent> GenerateEvents(CreateExampleAggregate request)
        {
            yield return new ExampleAggregateCreated(GuidGenerator.GenerateTimeBasedGuid(),request.AggregateRootId)
            {
                Name = request.Name,
                Date = request.Date,
                Number = request.Number,
            };
        }



        public async Task<IEnumerable<IMessaging>> Handle(ExampleAggregateCreated notification, CancellationToken cancellationToken)
        {
            this.Name = notification.Name;
            this.Date = notification.Date;
            this.Number = notification.Number;
            this.Id = notification.AggregateRootId;

            return new IMessaging[0];
        }
        
    }
    
}
