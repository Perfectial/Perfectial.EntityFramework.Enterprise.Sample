namespace Perfectial.UnitTests.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using FizzWare.NBuilder;

    using NUnit.Framework;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence;
    using Perfectial.Infrastructure.Persistence.Base;
    using Perfectial.Infrastructure.Persistence.EntityFramework;

    [TestFixture]
    public class DbContextScopeTest
    {
        private const int NumberOfEntitiesToCreate = 10;

        private readonly RandomGenerator daysGenerator = new RandomGenerator();
        private readonly RandomGenerator enumGenerator = new RandomGenerator();

        private IDbContextScopeFactory dbContextScopeFactory;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            this.dbContextScopeFactory = new DbContextScopeFactory();

            Database.SetInitializer(new ApplicationDbInitializer());
        }

        [TestFixtureTearDown]
        public void TestCleanup()
        {
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "You cannot call SaveChanges() more than once", MatchType = MessageMatch.StartsWith)]
        public void ShouldFailWhenSavingChangesMoreThanOnce()
        {
            var entity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(entity);
                dbContextScope.SaveChanges();
                dbContextScope.SaveChanges();

                Assert.Fail();
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "You cannot call SaveChanges() more than once", MatchType = MessageMatch.StartsWith)]
        public async Task ShouldFailWhenSavingChangesMoreThanOnceAsync()
        {
            var entity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(entity);

                await dbContextScope.SaveChangesAsync();
                await dbContextScope.SaveChangesAsync();

                Assert.Fail();
            }
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ShouldFailWhenSavingChangesOnDisposedDbContext()
        {
            var entity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(entity);

                dbContextScope.Dispose();
                dbContextScope.SaveChanges();

                Assert.Fail();
            }
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public async Task ShouldFailWhenSavingChangesOnDisposedDbContextAsync()
        {
            var entity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(entity);

                dbContextScope.Dispose();
                await dbContextScope.SaveChangesAsync();

                Assert.Fail();
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot join an ambient DbContextScope when an explicit database transaction is required", MatchType = MessageMatch.StartsWith)]
        public void ShouldFailWhenJoiningToDbTransaction()
        {
            using (var dbContextScope = new DbContextScope(DbContextScopeOption.JoinExisting, false, IsolationLevel.ReadUncommitted, null))
            {
                Assert.Fail();
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot nest a read/write DbContextScope within a read-only DbContextScope.", MatchType = MessageMatch.StartsWith)]
        public void ShouldFailWhenNestingReadWriteScopeInReadOnlyScope()
        {
            using (var dbReadOnlyContextScope = new DbContextScope(true, null))
            {
                using (var dbReadWriteContextScope = new DbContextScope(null))
                {
                    Assert.Fail();
                }
            }
        }

        [Test]
        public void ShouldAddEntity()
        {
            /*
            * Typical usage of DbContextScope for a read-write business transaction. 
            * It's as simple as it looks.
            */

            var entity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(entity);

                dbContextScope.SaveChanges();

                Assert.IsTrue(entity.Id != 0);
            }
        }

        [Test]
        public async Task ShouldAddEntityAsync()
        {
            var entity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(entity);

                await dbContextScope.SaveChangesAsync();

                Assert.IsTrue(entity.Id != 0);
            }
        }

        [Test]
        public void ShouldAddEntities()
        {
            /*
            * Example of DbContextScope nesting in action. 
            * 
            * We already have a service method - CreateUser() - that knows how to create a new user
            * and implements all the business rules around the creation of a new user 
            * (e.g. validation, initialization, sending notifications to other domain model objects...).
            * 
            * So we'll just call it in a loop to create the list of new users we've 
            * been asked to create.
            * 
            * Of course, since this is a business logic service method, we are making 
            * an implicit guarantee to whoever is calling us that the changes we make to 
            * the system will be either committed or rolled-back in an atomic manner. 
            * I.e. either all the users we've been asked to create will get persisted
            * or none of them will. It would be disastrous to have a partial failure here
            * and end up with some users but not all having been created.
            * 
            * DbContextScope makes this trivial to implement. 
            * 
            * The inner DbContextScope instance that the CreateUser() method creates
            * will join our top-level scope. This ensures that the same DbContext instance is
            * going to be used throughout this business transaction.
            * 
            */

            var entities = this.CreateEntities(NumberOfEntitiesToCreate);
            using (var dbParentContextScope = this.dbContextScopeFactory.Create())
            {
                foreach (var entity in entities)
                {
                    using (var dbNestedContextScope = this.dbContextScopeFactory.Create())
                    {
                        var dbContext = dbNestedContextScope.DbContexts.Get<ApplicationDbContext>();
                        dbContext.ToDoItems.Add(entity);

                        dbNestedContextScope.SaveChanges();

                        Assert.IsTrue(entity.Id == 0);
                    }
                }

                dbParentContextScope.SaveChanges();

                foreach (var entity in entities)
                {
                    Assert.IsTrue(entity.Id != 0);
                }
            }

            var entityIds = entities.Select(e => e.Id).ToList();
            using (var dbContextScope = this.dbContextScopeFactory.CreateReadOnly())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                var actualEntities = dbContext.ToDoItems.AsQueryable().Where(entity => entityIds.Contains(entity.Id));

                Assert.AreEqual(entities.Count, actualEntities.Count());
            }
        }

        [Test]
        public void ShouldNotAddEntitiesOnFailure()
        {
            /*
            * Here, we'll verify that inner DbContextScopes really join the parent scope and 
            * don't persist their changes until the parent scope completes successfully. 
            */

            var entities = this.CreateEntities(NumberOfEntitiesToCreate);
            try
            {
                using (var dbParentContextScope = this.dbContextScopeFactory.Create())
                {
                    var index = 0;
                    foreach (var entity in entities)
                    {
                        using (var dbNestedContextScope = this.dbContextScopeFactory.Create())
                        {
                            if (index++ < NumberOfEntitiesToCreate / 2)
                            {
                                var dbContext = dbNestedContextScope.DbContexts.Get<ApplicationDbContext>();
                                dbContext.ToDoItems.Add(entity);

                                dbNestedContextScope.SaveChanges();

                                Assert.IsTrue(entity.Id == 0);
                            }
                            else
                            {
                                // OK. So we've successfully persisted one user.
                                // We're going to simulate a failure when attempting to 
                                // persist the second user and see what ends up getting 
                                // persisted in the DB.
                                throw new InvalidOperationException($"An error occurred when attempting to create entity named '{entity.Description}' in the database.");
                            }
                        }
                    }

                    dbParentContextScope.SaveChanges();

                    foreach (var entity in entities)
                    {
                        Assert.IsTrue(entity.Id != 0);
                    }
                }

                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            var entityIds = entities.Select(e => e.Id).ToList();
            using (var dbContextScope = this.dbContextScopeFactory.CreateReadOnly())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                var actualEntities = dbContext.ToDoItems.AsQueryable().Where(entity => entityIds.Contains(entity.Id));

                Assert.AreEqual(0, actualEntities.Count());
            }
        }

        [Test]
        public void ShouldNotAddEntitiesInDbTransactionOnFailure()
        {
            /*
            * Here, we'll verify that inner DbContextScopes really join the parent scope and 
            * don't persist their changes until the parent scope completes successfully. 
            */

            var entities = this.CreateEntities(NumberOfEntitiesToCreate);
            try
            {
                using (var dbContextParentScope = this.dbContextScopeFactory.CreateWithTransaction(IsolationLevel.ReadUncommitted))
                {
                    var index = 0;
                    foreach (var entity in entities)
                    {
                        using (var dbContextNestedScope = this.dbContextScopeFactory.Create())
                        {
                            if (index++ < NumberOfEntitiesToCreate / 2)
                            {
                                var dbContext = dbContextNestedScope.DbContexts.Get<ApplicationDbContext>();
                                dbContext.ToDoItems.Add(entity);

                                dbContextNestedScope.SaveChanges();

                                Assert.IsTrue(entity.Id == 0);
                            }
                            else
                            {
                                // OK. So we've successfully persisted one user.
                                // We're going to simulate a failure when attempting to 
                                // persist the second user and see what ends up getting 
                                // persisted in the DB.
                                throw new InvalidOperationException($"An error occurred when attempting to create entity named '{entity.Description}' in the database.");
                            }
                        }
                    }

                    dbContextParentScope.SaveChanges();

                    foreach (var entity in entities)
                    {
                        Assert.IsTrue(entity.Id != 0);
                    }
                }

                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            var entityIds = entities.Select(e => e.Id).ToList();
            using (var dbContextScope = this.dbContextScopeFactory.CreateReadOnlyWithTransaction(IsolationLevel.ReadUncommitted))
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                var actualEntities = dbContext.ToDoItems.AsQueryable().Where(entity => entityIds.Contains(entity.Id));
                Assert.AreEqual(0, actualEntities.Count());
            }
        }

        [Test]
        public async Task ShouldGetEntitiesAsync()
        {
            /*
            * A very contrived example of ambient DbContextScope within an async flow.
            * 
            * Note that the ConfigureAwait(false) calls here aren't strictly necessary 
            * and are unrelated to DbContextScope. You can remove them if you want and 
            * the code will run in the same way. It is however good practice to configure
            * all your awaitables in library code to not continue 
            * on the captured synchronization context. It avoids having to pay the overhead 
            * of capturing the sync context and running the task continuation on it when 
            * library code doesn't need that context. If also helps prevent potential deadlocks 
            * if the upstream code has been poorly written and blocks on async tasks. 
            * 
            * "Library code" is any code in layers under the presentation tier. Typically any code
            * other that code in ASP.NET MVC / WebApi controllers or Window Form / WPF forms.
            * 
            * See http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx for 
            * more details.
            */

            var originalEntities = this.CreateEntities(NumberOfEntitiesToCreate);
            using (var dbContextParentScope = this.dbContextScopeFactory.Create())
            {
                foreach (var entity in originalEntities)
                {
                    using (var dbContextNestedScope = this.dbContextScopeFactory.Create())
                    {
                        var dbContext = dbContextNestedScope.DbContexts.Get<ApplicationDbContext>();
                        dbContext.ToDoItems.Add(entity);

                        dbContextNestedScope.SaveChanges();

                        Assert.IsTrue(entity.Id == 0);
                    }
                }

                dbContextParentScope.SaveChanges();

                foreach (var entity in originalEntities)
                {
                    Assert.IsTrue(entity.Id != 0);
                }
            }

            var entityIds = originalEntities.Select(e => e.Id).ToList();
            var actualEntities = new List<ToDoItem>();
            using (var dbContextScope = new DbContextReadOnlyScope())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                foreach (var entityId in entityIds)
                {
                    var entity = await dbContext.ToDoItems.FirstOrDefaultAsync(e => e.Id == entityId).ConfigureAwait(false);

                    // We're now in the continuation of the first async task. This is most
                    // likely executing in a thread from the ThreadPool, i.e. in a different
                    // thread that the one where we created our DbContextScope. Our ambient
                    // DbContextScope is still available here however, which allows the call 
                    // below to succeed.
                    actualEntities.Add(entity);
                }
            }

            Assert.AreEqual(originalEntities.Count, actualEntities.Count());
        }

        [Test]
        public void ShouldGetEntityInDbTransaction()
        {
            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.CreateWithTransaction(IsolationLevel.ReadUncommitted))
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(originalEntity);

                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            /*
            * An example of explicit database transaction. 
            * 
            * Read the comment for CreateReadOnlyWithTransaction() before using this overload
            * as there are gotchas when doing this!
            */

            using (var dbContextScope = this.dbContextScopeFactory.CreateReadOnlyWithTransaction(IsolationLevel.ReadUncommitted))
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                var actualEntity = dbContext.ToDoItems.AsQueryable().FirstOrDefault(e => e.Id == originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public async Task ShouldGetEntityInDbTransactionAsync()
        {
            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.CreateWithTransaction(IsolationLevel.ReadUncommitted))
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(originalEntity);

                await dbContextScope.SaveChangesAsync();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            /*
            * An example of explicit database transaction. 
            * 
            * Read the comment for CreateReadOnlyWithTransaction() before using this overload
            * as there are gotchas when doing this!
            */

            using (var dbContextScope = new DbContextReadOnlyScope(IsolationLevel.ReadUncommitted))
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                var actualEntity = await dbContext.ToDoItems.AsQueryable().FirstOrDefaultAsync(e => e.Id == originalEntity.Id);

                Assert.IsNotNull(actualEntity);
                Assert.AreEqual(originalEntity, actualEntity);
            }
        }

        [Test]
        public void ShouldForceEntityPersistenceInNestedScope()
        {
            // An example of disabling the DbContextScope nesting behaviour in order to force the persistence of changes made to entities
            // This is a pretty advanced feature that you can safely ignore until you actually need it.

            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(originalEntity);

                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            using (var dbParentContextScope = this.dbContextScopeFactory.Create())
            {
                var dbParentContext = dbParentContextScope.DbContexts.Get<ApplicationDbContext>();
                var actualEntityInParentScope = dbParentContext.ToDoItems.AsQueryable().FirstOrDefault(e => e.Id == originalEntity.Id);

                Assert.IsNotNull(actualEntityInParentScope);
                Assert.AreEqual(originalEntity, actualEntityInParentScope);

                var actualEntityDescription = actualEntityInParentScope.Description;

                /*
                * Demo of forcing the creation of a new DbContextScope
                * to ensure that changes made to the model in this service 
                * method are persisted even if that method happens to get
                * called within the scope of a wider business transaction
                * that eventually fails for any reason.
                * 
                * This is an advanced feature that should be used as rarely 
                * as possible (and ideally, never).
                */

                // We're going to send a welcome email to the provided user
                // (if one hasn't been sent already). Once sent, we'll update
                // that User entity in our DB to record that its Welcome email
                // has been sent.

                // Emails can't be rolled-back. Once they're sent, they're sent. 
                // So once the email has been sent successfully, we absolutely 
                // must persist this fact in our DB. Even if that method is called
                // by another busines logic service method as part of a wider 
                // business transaction and even if that parent business transaction
                // ends up failing for any reason, we still must ensure that
                // we have recorded the fact that the Welcome email has been sent.
                // Otherwise, we would risk spamming our users with repeated Welcome
                // emails. 

                // Force the creation of a new DbContextScope so that the changes we make here are
                // guaranteed to get persisted regardless of what happens after this method has completed.

                using (var dbNestedContextScope = this.dbContextScopeFactory.Create(DbContextScopeOption.ForceCreateNew))
                {
                    var dbNestedContext = dbNestedContextScope.DbContexts.Get<ApplicationDbContext>();
                    var actualEntityInNestedScope = dbNestedContext.ToDoItems.AsQueryable().FirstOrDefault(e => e.Id == originalEntity.Id);

                    Assert.IsNotNull(actualEntityInNestedScope);
                    Assert.AreEqual(originalEntity, actualEntityInNestedScope);

                    actualEntityInNestedScope.Description = Faker.Lorem.Sentence();

                    dbNestedContextScope.SaveChanges();

                    // When you force the creation of a new DbContextScope, you must force the parent
                    // scope (if any) to reload the entities you've modified here. Otherwise, the method calling
                    // you might not be able to see the changes you made here.
                    dbNestedContextScope.RefreshEntitiesInParentScope(new List<ToDoItem> { actualEntityInNestedScope });

                    Assert.AreEqual(actualEntityInNestedScope, actualEntityInParentScope);
                    Assert.AreNotEqual(actualEntityInNestedScope.Description, actualEntityDescription);
                    Assert.AreNotEqual(actualEntityInParentScope.Description, actualEntityDescription);
                }
            }
        }

        [Test]
        public async Task ShouldForceEntityPersistenceInNestedScopeAsync()
        {
            // An example of disabling the DbContextScope nesting behaviour in order to force the persistence of changes made to entities
            // This is a pretty advanced feature that you can safely ignore until you actually need it.

            var originalEntity = this.CreateEntity();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.ToDoItems.Add(originalEntity);

                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            using (var dbParentContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbParentContextScope.DbContexts.Get<ApplicationDbContext>();
                var actualEntityInParentScope = dbContext.ToDoItems.AsQueryable().FirstOrDefault(e => e.Id == originalEntity.Id);

                Assert.IsNotNull(actualEntityInParentScope);
                Assert.AreEqual(originalEntity, actualEntityInParentScope);

                var actualEntityDescription = actualEntityInParentScope.Description;

                /*
                * Demo of forcing the creation of a new DbContextScope
                * to ensure that changes made to the model in this service 
                * method are persisted even if that method happens to get
                * called within the scope of a wider business transaction
                * that eventually fails for any reason.
                * 
                * This is an advanced feature that should be used as rarely 
                * as possible (and ideally, never).
                */

                // We're going to send a welcome email to the provided user
                // (if one hasn't been sent already). Once sent, we'll update
                // that User entity in our DB to record that its Welcome email
                // has been sent.

                // Emails can't be rolled-back. Once they're sent, they're sent. 
                // So once the email has been sent successfully, we absolutely 
                // must persist this fact in our DB. Even if that method is called
                // by another busines logic service method as part of a wider 
                // business transaction and even if that parent business transaction
                // ends up failing for any reason, we still must ensure that
                // we have recorded the fact that the Welcome email has been sent.
                // Otherwise, we would risk spamming our users with repeated Welcome
                // emails. 

                // Force the creation of a new DbContextScope so that the changes we make here are
                // guaranteed to get persisted regardless of what happens after this method has completed.

                using (var dbNestedContextScope = this.dbContextScopeFactory.Create(DbContextScopeOption.ForceCreateNew))
                {
                    var dbNestedContext = dbNestedContextScope.DbContexts.Get<ApplicationDbContext>();
                    var actualEntityInNestedScope = dbNestedContext.ToDoItems.AsQueryable().FirstOrDefault(e => e.Id == originalEntity.Id);

                    Assert.IsNotNull(actualEntityInNestedScope);
                    Assert.AreEqual(originalEntity, actualEntityInNestedScope);

                    actualEntityInNestedScope.Description = Faker.Lorem.Sentence();

                    dbNestedContextScope.SaveChanges();

                    // When you force the creation of a new DbContextScope, you must force the parent
                    // scope (if any) to reload the entities you've modified here. Otherwise, the method calling
                    // you might not be able to see the changes you made here.

                    await dbNestedContextScope.RefreshEntitiesInParentScopeAsync(new List<ToDoItem> { actualEntityInNestedScope });

                    Assert.AreEqual(actualEntityInNestedScope, actualEntityInParentScope);
                    Assert.AreNotEqual(actualEntityInNestedScope.Description, actualEntityDescription);
                    Assert.AreNotEqual(actualEntityInParentScope.Description, actualEntityDescription);
                }
            }
        }

        [Test]
        public void ShouldAllowParallelProgramming()
        {
            var entities = this.CreateEntities(NumberOfEntitiesToCreate);
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.Set<ToDoItem>().AddRange(entities);

                dbContextScope.SaveChanges();

                foreach (var entity in entities)
                {
                    Assert.IsTrue(entity.Id != 0);
                }
            }

            // We're going to imagine that calculating a rank of a software engineer takes some time. 
            // So we'll do it in parallel.

            // You MUST call SuppressAmbientContext() when kicking off a parallel execution flow 
            // within a DbContextScope. Otherwise, this DbContextScope will remain the ambient scope
            // in the parallel flows of execution, potentially leading to multiple threads
            // accessing the same DbContext instance.

            var entityIds = entities.Select(e => e.Id).ToList();
            using (this.dbContextScopeFactory.SuppressAmbientContext())
            {
                Parallel.ForEach(entityIds, this.UpdateEntityState);
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

        private void UpdateEntityState(int entityId)
        {
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                var entity = dbContext.ToDoItems.AsQueryable().FirstOrDefault(e => e.Id == entityId);
                Assert.IsNotNull(entity);

                // Simulate the calculation of a task state taking some time.

                entity.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length);
                dbContextScope.SaveChanges();
            }
        }
    }
}
