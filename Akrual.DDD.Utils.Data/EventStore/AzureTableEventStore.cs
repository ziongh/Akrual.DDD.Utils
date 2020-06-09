using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Cache;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents.SpecialEvents;
using Akrual.DDD.Utils.Domain.Utils.TypeCache;
using Akrual.DDD.Utils.Internal.Extensions;
using Akrual.DDD.Utils.Internal.Serializer;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using Microsoft.Azure.Cosmos.Table;
using Streamstone;

namespace Akrual.DDD.Utils.Data.EventStore
{
    public class AzureTableEventEntry : TableEntity, IRecordedEvent
    {
        public Guid Id   { get; set; }
        public string Type { get; set; }
        public byte[] Data { get; set; }
        public IDomainEvent Event { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    public class AzureTableEventStore : IEventStore
    {
        private static readonly string ClientName = Environment.GetEnvironmentVariable("client", EnvironmentVariableTarget.Process).ToString();

        private static readonly string StorageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString", EnvironmentVariableTarget.Process).ToString();

        private readonly CloudStorageAccount _storageAccount;
        private readonly IDomainTypeFinder typeFinder;

        public AzureTableEventStore(IDomainTypeFinder typeFinder)
        {
            this.typeFinder = typeFinder;
#if DEBUG
            _storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
#else
            _storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
#endif
        }

        private async Task<CloudTable> GetTable(string streamName)
        {
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            var compoundName = streamName + $"_{ClientName}";
            CloudTable table = tableClient.GetTableReference(compoundName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", compoundName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", compoundName);
            }

            return table;
        }

        public async Task<Stream> GetStreamFromTable(CloudTable table, string streamKey)
        {
            var stream = await Stream.ProvisionAsync(new Partition(table, streamKey){ });

            return stream;
        }

        public async IAsyncEnumerable<StreamSlice<AzureTableEventEntry>> ReadAllEvents(Stream stream)
        {
            var results = await Stream.ReadAsync<AzureTableEventEntry>(stream.Partition);

            yield return results;

            var count = results.Events.Count();

            while (!results.IsEndOfStream)
            {
                results = await Stream.ReadAsync<AzureTableEventEntry>(stream.Partition, count);
                yield return results;
                count += results.Events.Count();
            }
        }


        public async Task<IEventStream> GetEventStream<T>(T obj) where T : IAggregateRoot
        {
            var id = obj.Id;

            var table = await GetTable(obj.StreamBaseName);
            var stream = await GetStreamFromTable(table, id.ToString("D"));

            var events = ReadAllEvents(stream);

            var allEvents = new List<AzureTableEventEntry>();
            await foreach(var eventSlice in events)
            {
                foreach (var eventSerialized in eventSlice.Events)
                {
                    var typeFromEvent = typeFinder.GetTypeFromString(eventSerialized.Type);
                    eventSerialized.Event = MessagePackSerializerLz4.Instance.Deserialize(typeFromEvent, eventSerialized.Data) as IDomainEvent;
                    eventSerialized.CreatedAt = UnixTimeStampToDateTime(eventSerialized.Timestamp.ToUnixTimeSeconds());
                }
                allEvents.AddRange(eventSlice.Events);
            }

            var streamName = typeof(T).FullName + "#" + obj.Id;

            var streamFinal = new EventStream
            {
                StreamName = streamName,
                Events = Task.FromResult(allEvents.Cast<IRecordedEvent>().AsEnumerable())
            };

            return streamFinal;
        }

        private static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
            return dtDateTime;
        }

        
        public async Task SaveNewEvents(Dictionary<EventStreamNameComponents, IEnumerable<IDomainEvent>> allEventsFromAggregates)
        {
            foreach (var eventsFromAggregate in allEventsFromAggregates)
            {
                var id = eventsFromAggregate.Key.AggregateGuid;

                var table = await GetTable(eventsFromAggregate.Key.StreamBaseName);
                var stream = await GetStreamFromTable(table, id.ToString("D"));
                var newEvents  = new List<EventData>();
                
                foreach (var @event in eventsFromAggregate.Value)
                {
                    newEvents.Add(CreateEvent(@event));
                }
                await Stream.WriteAsync(stream, newEvents.ToArray());
            }
        }


        public EventData CreateEvent<T>(T @event) where T : IDomainEvent
        {
            var serializedData = MessagePackSerializerLz4.Instance.Serialize(@event);
            var properties = new Dictionary<string, EntityProperty>
            {
                {"Id",   new EntityProperty(@event.EventGuid)},
                {"Type", new EntityProperty(typeof(T).FullName)},
                {"CreatedAt", new EntityProperty(DateTime.UtcNow)},
                {"Data", new EntityProperty(serializedData)}
            };

            return new EventData(EventId.From(@event.EventGuid), EventProperties.From(properties));
        }
    }
}