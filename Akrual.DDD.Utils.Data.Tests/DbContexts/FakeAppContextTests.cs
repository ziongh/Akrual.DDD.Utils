using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akrual.DDD.Utils.Data.DbContexts;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using Xunit;

namespace Akrual.DDD.Utils.Data.Tests.DbContexts
{
     public class FakeAppContextTests
    {
        [Fact()]
        public async Task Creator_GeneratesListsForTypes_ReturnEmptyList()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var blankPus = repository.FindBy<ExampleAggregate>(s => true).ToList();
            Assert.NotNull(blankPus);
            Assert.Empty(blankPus);
        }

        [Fact()]
        public async Task Upsert_TestPUDiario_ReturnInsertedItemsAndInsertsIntoDB()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var factory = new FactoryBaseWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());

            repository.AddOrUpdate(exampleAggregate);

            var newPus = repository.FindBy<ExampleAggregate>(s => true).ToList();

            Assert.Equal(1, newPus.Count);
            Assert.Equal(newPus[0],exampleAggregate);
        }

        [Fact()]
        public async Task FindBy_AfterTwoInserts_ReturnTwoInsertedItems()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);
            
            var factory = new FactoryBaseWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate);

            var exampleAggregate2 = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate2);

            var newPus2 = repository.FindBy<ExampleAggregate>(s => true).ToList();

            Assert.True(newPus2.Count == 2);
            Assert.Equal(newPus2[0], exampleAggregate);
            Assert.Equal(newPus2[1], exampleAggregate2);
        }

        [Fact()]
        public async Task All_AfterTwoInserts_ReturnTwoInsertedItems()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var factory = new FactoryBaseWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate);

            var exampleAggregate2 = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate2);

            var newPus2 = repository.All<ExampleAggregate>().ToList();

            Assert.True(newPus2.Count == 2);
            Assert.Equal(newPus2[0], exampleAggregate);
            Assert.Equal(newPus2[1], exampleAggregate2);
        }

        [Fact()]
        public async Task Remove_AfterTwoInsertsAndOneDelete_ReturnFirstInsertedItem()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var factory = new FactoryBaseWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate);

            var guid2 = GuidGenerator.GenerateTimeBasedGuid();
            var exampleAggregate2 = await factory.Create(guid2);
            repository.AddOrUpdate(exampleAggregate2);

            var entitiesToRemove = repository.FindBy<ExampleAggregate>(s => s.Id == guid2).ToList();
            repository.RemoveRange<ExampleAggregate>(entitiesToRemove);

            var newPus3 = repository.FindBy<ExampleAggregate>(s => true).ToList();


            Assert.True(newPus3.Count ==1);
            Assert.Equal(newPus3[0], exampleAggregate);
        }

        [Fact()]
        public async Task GetById_AfterTwoInserts_ReturnCorrectEntryByIDInsertedItem()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var factory = new FactoryBaseWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate);

            var guid2 = GuidGenerator.GenerateTimeBasedGuid();
            var exampleAggregate2 = await factory.Create(guid2);
            repository.AddOrUpdate(exampleAggregate2);


            var dbEntry = await repository.GetByIdAsync<ExampleAggregate>(guid2);

            Assert.Equal(dbEntry, exampleAggregate2);
        }

        [Fact()]
        public async Task GetById_AfterTwoInsertsSearchNonExistingID_ReturnNull()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var factory = new FactoryBaseWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate);

            var guid2 = GuidGenerator.GenerateTimeBasedGuid();
            var exampleAggregate2 = await factory.Create(guid2);
            repository.AddOrUpdate(exampleAggregate2);
            var guid3 = GuidGenerator.GenerateTimeBasedGuid();

            var dbEntry = await repository.GetByIdAsync<ExampleAggregate>(guid3);

            Assert.Null(dbEntry);
        }



        [Fact()]
        public async Task ClearDB_AfterTwoInsertsAndClearSearchExistingID_ReturnNull()
        {
            var repository = new FakeAppContext(typeof(ExampleAggregate).Assembly);

            var factory = new FactoryBaseWithDefaultObjectCreation();
            var exampleAggregate = await factory.Create(GuidGenerator.GenerateTimeBasedGuid());
            repository.AddOrUpdate(exampleAggregate);

            var guid2 = GuidGenerator.GenerateTimeBasedGuid();
            var exampleAggregate2 = await factory.Create(guid2);
            repository.AddOrUpdate(exampleAggregate2);

            repository.ClearDB();

            var dbEntry = await repository.GetByIdAsync<ExampleAggregate>(guid2);

            Assert.Null(dbEntry);
        }
    }
}
