using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Data.EventStore.Logger;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Akrual.DDD.Utils.Data.EventStore
{
    public enum TcpType
    {
        Normal,
        Ssl
    }

    public class EventStore
    {
        public async Task method()
        {
            var connection = EventStoreConnection.Create(Settings(TcpType.Normal, new UserCredentials("akrual", "akrual")).Build());
            await connection.ConnectAsync();

            var myEvent = new EventData(Guid.NewGuid(), "testEvent", false,
                Encoding.UTF8.GetBytes("some data"),
                Encoding.UTF8.GetBytes("some metadata"));

            var result = await connection.AppendToStreamAsync("test-stream",
                ExpectedVersion.Any, myEvent);

            var streamEvents = await connection.ReadStreamEventsForwardAsync("test-stream", 0, 1, false);

            var returnedEvent = streamEvents.Events[0].Event;

            Console.WriteLine("Read event with data: {0}, metadata: {1}",
                Encoding.UTF8.GetString(returnedEvent.Data),
                Encoding.UTF8.GetString(returnedEvent.Metadata));

        }


        private static ConnectionSettingsBuilder Settings(TcpType tcpType, UserCredentials userCredentials)
        {

            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(userCredentials)
                .UseDebugLogger()
                .EnableVerboseLogging()
                .LimitReconnectionsTo(10)
                .LimitRetriesForOperationTo(100)
                .SetTimeoutCheckPeriodTo(TimeSpan.FromMilliseconds(100))
                .SetReconnectionDelayTo(TimeSpan.Zero)
                .FailOnNoServerResponse()
                .SetGossipSeedEndPoints(
                    new GossipSeed(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113)),
                    new GossipSeed(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2113)),
                    new GossipSeed(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3113))
                    )
                .SetOperationTimeoutTo(TimeSpan.FromDays(1));
            if (tcpType == TcpType.Ssl)
                settings.UseSslConnection("ES", false);
            return settings;
        }
    }
}
