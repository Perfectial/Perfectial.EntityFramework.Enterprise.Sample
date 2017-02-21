namespace Perfectial.UnitTests.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;

    using FizzWare.NBuilder;

    using NUnit.Framework;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence;
    using Perfectial.Infrastructure.Persistence.Base;
    using Perfectial.Infrastructure.Persistence.EntityFramework;

    [TestFixture]
    public class RepositoryTest
    {
        private const int NumberOfEntitiesToCreate = 10;
        private const int NonExistingEntityId = -1;

        private readonly RandomGenerator daysGenerator = new RandomGenerator();
        private readonly RandomGenerator enumGenerator = new RandomGenerator();

        private IAmbientDbContextLocator ambientDbContextLocator;
        private IDbContextScopeFactory dbContextScopeFactory;

        private Repository<ApplicationDbContext, ToDoItem, int> repository;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            this.ambientDbContextLocator = new AmbientDbContextLocator();
            this.dbContextScopeFactory = new DbContextScopeFactory();

            this.repository = new Repository<ApplicationDbContext, ToDoItem, int>(this.ambientDbContextLocator);

            Database.SetInitializer(new ApplicationDbInitializer());
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.Database.Initialize(true);
            }
        }

        [TestFixtureTearDown]
        public void TestCleanup()
        {
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "No ambient DbContext of type TDbContext found", MatchType = MessageMatch.StartsWith)]
        public void ShouldFailWhenUsingRepositoryOutsideDbContextScope()
        {
            this.repository.GetAll();
        }

        [Test]
        public void ShouldGetAll()
        {
            this.AddEntities();

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntities = this.repository.GetAll();
                Assert.IsTrue(actualEntities.Any());
            }
        }

        [Test]
        public void ShouldGetAllList()
        {
            this.AddEntities();

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntities = this.repository.GetAllList();
                Assert.IsTrue(actualEntities.Any());
            }
        }

        [Test]
        public async Task ShouldGetAllListAsync()
        {
            this.AddEntities();

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntities = await this.repository.GetAllListAsync();
                Assert.IsTrue(actualEntities.Any());
            }
        }

        [Test]
        public void ShouldGetAllListWithPredidate()
        {
            var originalEntities = this.CreateEntities(NumberOfEntitiesToCreate);
            var entityDescription = Faker.Lorem.Sentence();
            var index = 0;
            originalEntities.ForEach(
                entity =>
                {
                    if (index++ % 2 == 0)
                    {
                        entity.Description = entityDescription;
                    }
                });

            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.repository.AddRange(originalEntities.ToArray());
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntities = this.repository.GetAllList(entity => entity.Description == entityDescription);
                Assert.AreEqual(NumberOfEntitiesToCreate / 2, actualEntities.Count());
            }
        }

        [Test]
        public async Task ShouldGetAllListWithPredicateAsync()
        {
            var originalEntities = this.CreateEntities(NumberOfEntitiesToCreate);
            var entityDescription = Faker.Lorem.Sentence();
            var index = 0;
            originalEntities.ForEach(
                entity =>
                {
                    if (index++ % 2 == 0)
                    {
                        entity.Description = entityDescription;
                    }
                });

            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.repository.AddRange(originalEntities.ToArray());
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntities = await this.repository.GetAllListAsync(entity => entity.Description == entityDescription);
                Assert.AreEqual(NumberOfEntitiesToCreate / 2, actualEntities.Count());
            }
        }

        [Test]
        public void ShouldGetById()
        {
            var originalEntity = this.AddEntity();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public async Task ShouldGetByIdAsync()
        {
            var originalEntity = this.AddEntity();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public void ShouldGetSingle()
        {
            var originalEntities = this.AddEntities();
            var originalEntity = originalEntities.First();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity = this.repository.Single(entity => entity.Id == originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public async Task ShouldGetSingleAsync()
        {
            var originalEntities = this.AddEntities();
            var originalEntity = originalEntities.First();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity = await this.repository.SingleAsync(entity => entity.Id == originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public void ShouldGetFirstOrDefault()
        {
            var originalEntities = this.AddEntities();
            var originalEntity = originalEntities.First();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualExistingEntity = this.repository.FirstOrDefault(originalEntity.Id);
                var actualNonExistingEntity = this.repository.FirstOrDefault(NonExistingEntityId);

                Assert.IsNotNull(actualExistingEntity);
                Assert.AreEqual(originalEntity, actualExistingEntity);

                Assert.IsNull(actualNonExistingEntity);
            }
        }

        [Test]
        public async Task ShouldGetFirstOrDefaultAsync()
        {
            var originalEntities = this.AddEntities();
            var originalEntity = originalEntities.First();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualExistingEntity = await this.repository.FirstOrDefaultAsync(originalEntity.Id);
                var actualNonExistingEntity = await this.repository.FirstOrDefaultAsync(NonExistingEntityId);

                Assert.IsNotNull(actualExistingEntity);
                Assert.AreEqual(originalEntity, actualExistingEntity);

                Assert.IsNull(actualNonExistingEntity);
            }
        }

        [Test]
        public void ShouldGetFirstOrDefaultWithPredicate()
        {
            var originalEntities = this.AddEntities();
            var originalEntity = originalEntities.First();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualExistingEntity = this.repository.FirstOrDefault(entity => entity.Id == originalEntity.Id);
                var actualNonExistingEntity = this.repository.FirstOrDefault(entity => entity.Id == NonExistingEntityId);

                Assert.IsNotNull(actualExistingEntity);
                Assert.AreEqual(originalEntity, actualExistingEntity);

                Assert.IsNull(actualNonExistingEntity);
            }
        }

        [Test]
        public async Task ShouldGetFirstOrDefaultWithPredicateAsync()
        {
            var originalEntities = this.AddEntities();
            var originalEntity = originalEntities.First();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualExistingEntity = await this.repository.FirstOrDefaultAsync(entity => entity.Id == originalEntity.Id);
                var actualNonExistingEntity = await this.repository.FirstOrDefaultAsync(entity => entity.Id == NonExistingEntityId);

                Assert.IsNotNull(actualExistingEntity);
                Assert.AreEqual(originalEntity, actualExistingEntity);

                Assert.IsNull(actualNonExistingEntity);
            }
        }

        [Test]
        public void ShouldAttachEntity()
        {
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var entity = this.CreateEntity();

                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();

                ObjectStateEntry objectStateEntry;
                if (((IObjectContextAdapter)dbContext).ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out objectStateEntry))
                {
                    Assert.Fail();
                }
                else
                {
                    this.repository.Attach(entity);

                    if (((IObjectContextAdapter)dbContext).ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out objectStateEntry))
                    {
                        Assert.AreEqual(EntityState.Unchanged, objectStateEntry.State);
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
            }
        }

        [Test]
        public void ShouldAdd()
        {
            var originalEntity = this.AddEntity();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public async Task ShouldAddAsync()
        {
            var originalEntity = await this.AddEntityAsync();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public void ShouldAddRange()
        {
            var originalEntity1 = this.CreateEntity();
            var originalEntity2 = this.CreateEntity();
            var originalEntity3 = this.CreateEntity();

            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.repository.AddRange(new List<ToDoItem> { originalEntity1, originalEntity2, originalEntity3 });
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity1.Id != 0);
                Assert.IsTrue(originalEntity2.Id != 0);
                Assert.IsTrue(originalEntity3.Id != 0);
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity1 = this.repository.Get(originalEntity1.Id);
                var actualEntity2 = this.repository.Get(originalEntity2.Id);
                var actualEntity3 = this.repository.Get(originalEntity3.Id);

                Assert.IsNotNull(actualEntity1);
                Assert.IsNotNull(actualEntity2);
                Assert.IsNotNull(actualEntity3);
                Assert.AreEqual(originalEntity1, actualEntity1);
                Assert.AreEqual(originalEntity2, actualEntity2);
                Assert.AreEqual(originalEntity3, actualEntity3);
            }
        }

        [Test]
        public async Task ShouldAddRangeAsync()
        {
            var originalEntity1 = this.CreateEntity();
            var originalEntity2 = this.CreateEntity();
            var originalEntity3 = this.CreateEntity();

            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                await this.repository.AddRangeAsync(new List<ToDoItem> { originalEntity1, originalEntity2, originalEntity3 });
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity1.Id != 0);
                Assert.IsTrue(originalEntity2.Id != 0);
                Assert.IsTrue(originalEntity3.Id != 0);
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntity1 = await this.repository.GetAsync(originalEntity1.Id);
                var actualEntity2 = await this.repository.GetAsync(originalEntity2.Id);
                var actualEntity3 = await this.repository.GetAsync(originalEntity3.Id);

                Assert.IsNotNull(actualEntity1);
                Assert.IsNotNull(actualEntity2);
                Assert.IsNotNull(actualEntity3);
                Assert.AreEqual(originalEntity1, actualEntity1);
                Assert.AreEqual(originalEntity2, actualEntity2);
                Assert.AreEqual(originalEntity3, actualEntity3);
            }
        }

        [Test]
        public void ShouldAddOrUpdate()
        {
            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.repository.AddOrUpdate(originalEntity);
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            ToDoItem actualEntity;
            var entityTitle = Faker.Lorem.Sentence();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                actualEntity.Description = entityTitle;
                this.repository.AddOrUpdate(actualEntity);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreNotEqual(originalEntity, actualEntity);
                Assert.AreEqual(entityTitle, actualEntity.Description);
            }
        }

        [Test]
        public async Task ShouldAddOrUpdateAsync()
        {
            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                await this.repository.AddOrUpdateAsync(originalEntity);
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            ToDoItem actualEntity;
            var entityTitle = Faker.Lorem.Sentence();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                actualEntity.Description = entityTitle;
                await this.repository.AddOrUpdateAsync(actualEntity);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreNotEqual(originalEntity, actualEntity);
                Assert.AreEqual(entityTitle, actualEntity.Description);
            }
        }

        [Test]
        public void ShouldUpdate()
        {
            var originalEntity = this.AddEntity();

            ToDoItem actualEntity;
            var entityTitle = Faker.Lorem.Sentence();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                actualEntity.Description = entityTitle;
                this.repository.Update(actualEntity);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreNotEqual(originalEntity, actualEntity);
                Assert.AreEqual(entityTitle, actualEntity.Description);
            }
        }

        [Test]
        public async Task ShouldUpdateAsync()
        {
            var originalEntity = await this.AddEntityAsync();

            ToDoItem actualEntity;
            var entityTitle = Faker.Lorem.Sentence();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                actualEntity.Description = entityTitle;
                await this.repository.UpdateAsync(actualEntity);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreNotEqual(originalEntity, actualEntity);
                Assert.AreEqual(entityTitle, actualEntity.Description);
            }
        }

        [Test]
        public void ShouldDelete()
        {
            var originalEntity = this.AddEntity();

            ToDoItem actualEntity;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                this.repository.Delete(actualEntity);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = this.repository.FirstOrDefault(originalEntity.Id);

                Assert.IsNull(actualEntity);
            }
        }

        [Test]
        public async Task ShouldDeleteAsync()
        {
            var originalEntity = await this.AddEntityAsync();

            ToDoItem actualEntity;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                await this.repository.DeleteAsync(actualEntity);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = await this.repository.FirstOrDefaultAsync(originalEntity.Id);

                Assert.IsNull(actualEntity);
            }
        }

        [Test]
        public void ShouldDeleteById()
        {
            var originalEntity = this.AddEntity();

            ToDoItem actualEntity;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                this.repository.Delete(actualEntity.Id);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = this.repository.FirstOrDefault(originalEntity.Id);

                Assert.IsNull(actualEntity);
            }
        }

        [Test]
        public async Task ShouldDeleteByIdAsync()
        {
            var originalEntity = await this.AddEntityAsync();

            ToDoItem actualEntity;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                await this.repository.DeleteAsync(actualEntity.Id);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = await this.repository.FirstOrDefaultAsync(originalEntity.Id);

                Assert.IsNull(actualEntity);
            }
        }

        [Test]
        public void ShouldDeleteByPredicate()
        {
            var originalEntity = this.AddEntity();

            ToDoItem actualEntity;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = this.repository.Get(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                var actualEntityId = actualEntity.Id;
                this.repository.Delete(entity => entity.Id == actualEntityId);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = this.repository.FirstOrDefault(originalEntity.Id);

                Assert.IsNull(actualEntity);
            }
        }

        [Test]
        public async Task ShouldDeleteByPredicateAsync()
        {
            var originalEntity = await this.AddEntityAsync();

            ToDoItem actualEntity;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity = await this.repository.GetAsync(originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);

                var actualEntityId = actualEntity.Id;
                await this.repository.DeleteAsync(entity => entity.Id == actualEntityId);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity = await this.repository.FirstOrDefaultAsync(originalEntity.Id);

                Assert.IsNull(actualEntity);
            }
        }

        [Test]
        public void ShouldDeleteRange()
        {
            var originalEntity1 = this.CreateEntity();
            var originalEntity2 = this.CreateEntity();
            var originalEntity3 = this.CreateEntity();

            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.repository.AddRange(new List<ToDoItem> { originalEntity1, originalEntity2, originalEntity3 });
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity1.Id != 0);
                Assert.IsTrue(originalEntity2.Id != 0);
                Assert.IsTrue(originalEntity3.Id != 0);
            }

            ToDoItem actualEntity1;
            ToDoItem actualEntity2;
            ToDoItem actualEntity3;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity1 = this.repository.Get(originalEntity1.Id);
                actualEntity2 = this.repository.Get(originalEntity2.Id);
                actualEntity3 = this.repository.Get(originalEntity3.Id);

                Assert.IsNotNull(actualEntity1);
                Assert.IsNotNull(actualEntity2);
                Assert.IsNotNull(actualEntity3);
                Assert.AreEqual(originalEntity1, actualEntity1);
                Assert.AreEqual(originalEntity2, actualEntity2);
                Assert.AreEqual(originalEntity3, actualEntity3);

                this.repository.DeleteRange(new List<ToDoItem> { actualEntity1, actualEntity2, actualEntity3 });
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity1 = this.repository.FirstOrDefault(originalEntity1.Id);
                actualEntity2 = this.repository.FirstOrDefault(originalEntity2.Id);
                actualEntity3 = this.repository.FirstOrDefault(originalEntity3.Id);

                Assert.IsNull(actualEntity1);
                Assert.IsNull(actualEntity2);
                Assert.IsNull(actualEntity3);
            }
        }

        [Test]
        public async Task ShouldDeleteRangeAsync()
        {
            var originalEntity1 = this.CreateEntity();
            var originalEntity2 = this.CreateEntity();
            var originalEntity3 = this.CreateEntity();

            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                await this.repository.AddRangeAsync(new List<ToDoItem> { originalEntity1, originalEntity2, originalEntity3 });
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity1.Id != 0);
                Assert.IsTrue(originalEntity2.Id != 0);
                Assert.IsTrue(originalEntity3.Id != 0);
            }

            ToDoItem actualEntity1;
            ToDoItem actualEntity2;
            ToDoItem actualEntity3;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualEntity1 = await this.repository.GetAsync(originalEntity1.Id);
                actualEntity2 = await this.repository.GetAsync(originalEntity2.Id);
                actualEntity3 = await this.repository.GetAsync(originalEntity3.Id);

                Assert.IsNotNull(actualEntity1);
                Assert.IsNotNull(actualEntity2);
                Assert.IsNotNull(actualEntity3);
                Assert.AreEqual(originalEntity1, actualEntity1);
                Assert.AreEqual(originalEntity2, actualEntity2);
                Assert.AreEqual(originalEntity3, actualEntity3);

                await this.repository.DeleteRangeAsync(new List<ToDoItem> { actualEntity1, actualEntity2, actualEntity3 });
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualEntity1 = await this.repository.FirstOrDefaultAsync(originalEntity1.Id);
                actualEntity2 = await this.repository.FirstOrDefaultAsync(originalEntity2.Id);
                actualEntity3 = await this.repository.FirstOrDefaultAsync(originalEntity3.Id);

                Assert.IsNull(actualEntity1);
                Assert.IsNull(actualEntity2);
                Assert.IsNull(actualEntity3);
            }
        }

        [Test]
        public void ShouldCount()
        {
            this.AddEntity();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var entitiesCount = this.repository.Count();

                Assert.IsTrue(entitiesCount > 0);
            }
        }

        [Test]
        public async Task ShouldCountAsync()
        {
            await this.AddEntityAsync();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var entitiesCount = await this.repository.CountAsync();

                Assert.IsTrue(entitiesCount > 0);
            }
        }

        [Test]
        public void ShouldCountWithPredicate()
        {
            var originalEntity = this.AddEntity();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var entitiesCount = this.repository.Count(entity => entity.Id == originalEntity.Id);

                Assert.AreEqual(1, entitiesCount);
            }
        }

        [Test]
        public async Task ShouldCountWithPredicateAsync()
        {
            var originalEntity = await this.AddEntityAsync();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var entitiesCount = await this.repository.CountAsync(entity => entity.Id == originalEntity.Id);

                Assert.AreEqual(1, entitiesCount);
            }
        }

        private ToDoItem CreateEntity()
        {
            var entity = Builder<ToDoItem>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.AssignedUserId = null)
                .With(e => e.AssignedUser = null)
                .With(e => e.CreationTime = DateTime.UtcNow.AddDays(-this.daysGenerator.Next(1, 365)))
                .With(e => e.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length))
                .With(e => e.Description = Faker.Lorem.Sentence())
                .Build();

            return entity;
        }

        private List<ToDoItem> CreateEntities(int numberOfEntities)
        {
            var entities = Builder<ToDoItem>.CreateListOfSize(numberOfEntities)
                .All()
                    .With(e => e.Id = 0)
                    .With(e => e.AssignedUserId = null)
                    .With(e => e.AssignedUser = null)
                    .With(e => e.CreationTime = DateTime.UtcNow.AddDays(-this.daysGenerator.Next(1, 365)))
                    .With(e => e.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length))
                    .With(e => e.Description = Faker.Lorem.Sentence())
                .Build().ToList();

            return entities;
        }

        private ToDoItem AddEntity()
        {
            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.repository.Add(originalEntity);
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            return originalEntity;
        }

        private async Task<ToDoItem> AddEntityAsync()
        {
            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                await this.repository.AddAsync(originalEntity);
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            return originalEntity;
        }

        private IEnumerable<ToDoItem> AddEntities()
        {
            var originalEntities = this.CreateEntities(NumberOfEntitiesToCreate);
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.repository.AddRange(originalEntities.ToArray());
                dbContextScope.SaveChanges();
            }

            return originalEntities;
        }
    }
}
