namespace Perfectial.UnitTests.Domain
{
    using System;

    using FizzWare.NBuilder;

    using NUnit.Framework;

    using Perfectial.Domain.Model;

    [TestFixture]
    public class DomainModelTest
    {
        private readonly RandomGenerator daysGenerator = new RandomGenerator();
        private readonly RandomGenerator enumGenerator = new RandomGenerator();

        [TestFixtureSetUp]
        public void TestSetup()
        {
        }

        [TestFixtureTearDown]
        public void TestCleanup()
        {
        }

        [Test]
        public void ShouldEntitiesBeEqual()
        {
            var entity1 = this.CreateEntity();
            var entity2 = this.Clone(entity1);
            var entity3 = this.CreateEntity();

            EntityBase<int> baseEntity1 = entity1;
            EntityBase<int> baseEntity2 = entity2;

            Assert.AreEqual(entity1, entity2);
            Assert.IsTrue(baseEntity1.Equals(baseEntity2));
            Assert.IsTrue(entity1 == entity2);

            var entity1HashCode = entity1.GetHashCode();
            var entity2HashCode = entity2.GetHashCode();
            var entity3HashCode = entity3.GetHashCode();

            Assert.AreEqual(entity1HashCode, entity2HashCode);
            Assert.AreNotEqual(entity1HashCode, entity3HashCode);
        }

        [Test]
        public void ShouldEntitiesNotBeEqual()
        {
            var entity1 = this.CreateEntity();
            Assert.IsFalse(entity1.Equals(null));
            Assert.IsFalse(entity1.Equals(new object()));

            var entity2 = entity1;
            Assert.IsTrue(entity1.Equals(entity2));

            var entity3 = this.CreateEntity();
            Assert.AreNotEqual(entity1, entity3);
            Assert.IsTrue(entity1 != entity3);
        }

        private ToDoItem CreateEntity()
        {
            var entity = Builder<ToDoItem>.CreateNew()
                .With(e => e.Id = 1)
                .With(e => e.AssignedUserId = string.Empty)
                .With(e => e.AssignedUser = null)
                .With(e => e.CreationTime = DateTime.UtcNow.AddDays(-this.daysGenerator.Next(1, 365)))
                .With(e => e.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length))
                .With(e => e.Description = Faker.Lorem.Sentence())
                .Build();

            return entity;
        }

        private ToDoItem Clone(ToDoItem entity)
        {
            var clonedEntity = new ToDoItem
            {
                Id = entity.Id,
                CreationTime = entity.CreationTime,
                State = entity.State,
                Description = entity.Description
            };

            return clonedEntity;
        }
    }
}
