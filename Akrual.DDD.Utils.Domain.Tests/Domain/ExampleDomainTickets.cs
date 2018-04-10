using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Domain.Messaging.Coordinator;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.TicketsReservation;
using Akrual.DDD.Utils.Domain.UOW;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class ExampleDomainTickets : TestsUsingSimpleInjector
    {
        [Fact]
        public async Task DispatchCreateCommand_ANewAggregateShouldBeInsertedOnUOW()
        {
            var coordinator = this._container.GetInstance<ICoordinator>();
            var id = GuidGenerator.GenerateTimeBasedGuid();
            var userId = GuidGenerator.GenerateTimeBasedGuid();

            var command = new _1PlaceOrder(id)
            {
                UserId = userId,
                SeatNumber = 10,
                CardNumber = "Creditcard",
                Value = 200
            };

            await coordinator.DispatchAndApplyEvents(command,CancellationToken.None);

            var uow = this._container.GetInstance<IUnitOfWork>();
            var listOfListOfAggregatesByType = uow.LoadedAggregates.ToList();

            Assert.Equal(4, listOfListOfAggregatesByType.Count);
            foreach (var listOfAggregatesOfSameType in listOfListOfAggregatesByType)
            {
                Assert.Single(listOfAggregatesOfSameType.Value);
            }


        }
    }
}
