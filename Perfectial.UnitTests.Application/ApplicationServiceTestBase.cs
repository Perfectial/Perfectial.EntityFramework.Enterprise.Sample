namespace Perfectial.UnitTests.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;

    using Common.Logging;

    using FizzWare.NBuilder;

    using NUnit.Framework;

    using Perfectial.Application.Model;
    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    using Rhino.Mocks;

    using ToDoItemState = Perfectial.Domain.Model.ToDoItemState;

    public class ApplicationServiceTestBase
    {
        private readonly RandomGenerator daysGenerator = new RandomGenerator();
        private readonly RandomGenerator enumGenerator = new RandomGenerator();

        protected MockRepository Repository { get; set; }

        protected IAmbientDbContextLocator AmbientDbContextLocator { get; set; }
        protected IDbContextScopeFactory DbContextScopeFactory { get; set; }

        protected IRepository<ToDoItem, int> ToDoItemRepository { get; set; }
        protected IRepository<User, string> UserRepository { get; set; }

        protected IMapper Mapper { get; set; }
        protected ILog Logger { get; set; }

        [TestFixtureSetUp]
        protected virtual void TestSetup()
        {
            /*var mapperConfiguration = new MapperConfiguration(
                configuration =>
                    {
                        configuration.CreateMap<User, UserDto>();
                        configuration.CreateMap<ToDoItem, ToDoItemDto>();
                    });

            this.Mapper = mapperConfiguration.CreateMapper();*/
        }

        [SetUp]
        protected virtual void Setup()
        {
            this.Repository = new MockRepository();

            this.AmbientDbContextLocator = this.Repository.StrictMock<IAmbientDbContextLocator>();
            this.DbContextScopeFactory = this.Repository.StrictMock<IDbContextScopeFactory>();

            this.ToDoItemRepository = this.Repository.StrictMock<IRepository<ToDoItem, int>>();
            this.UserRepository = this.Repository.StrictMock<IRepository<User, string>>();

            this.Mapper = this.Repository.StrictMock<IMapper>();
            this.Logger = this.Repository.StrictMock<ILog>();
            }

        [TestFixtureTearDown]
        protected virtual void TestCleanup()
        {
        }

        protected ToDoItem CreateToDoItem()
        {
            var toDoItem = Builder<ToDoItem>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.AssignedUserId = null)
                .With(e => e.AssignedUser = null)
                .With(e => e.CreationTime = DateTime.UtcNow.AddDays(-this.daysGenerator.Next(1, 365)))
                .With(e => e.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length))
                .With(e => e.Description = Faker.Lorem.Sentence())
                .Build();

            return toDoItem;
        }

        protected List<ToDoItem> CreateToDoItems(int numberOfToDoItems)
        {
            var entities = Builder<ToDoItem>.CreateListOfSize(numberOfToDoItems)
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

        protected List<ToDoItemDto> CreateToDoItemsDto(int numberOfToDoItems)
        {
            var entities = Builder<ToDoItemDto>.CreateListOfSize(numberOfToDoItems)
                .All()
                    .With(e => e.Id = 0)
                    .With(e => e.AssignedUserId = null)
                    .With(e => e.CreationTime = DateTime.UtcNow.AddDays(-this.daysGenerator.Next(1, 365)))
                    .With(e => e.State = (Perfectial.Application.Model.ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(Perfectial.Application.Model.ToDoItemState)).Length))
                    .With(e => e.Description = Faker.Lorem.Sentence())
                .Build().ToList();

            return entities;
        }

        protected User CreateUser()
        {
            var user = Builder<User>.CreateNew()
                .With(e => e.Id = string.Empty)
                .With(e => e.UserName = Faker.Name.FullName())
                .Build();

            return user;
        }

        protected List<User> CreateUsers(int numberOfUsers)
        {
            var users = Builder<User>.CreateListOfSize(numberOfUsers)
                .All()
                    .With(e => e.Id = string.Empty)
                    .With(e => e.UserName = Faker.Name.FullName())
                .Build().ToList();

            return users;
        }

        protected List<UserDto> CreateUsersDto(int numberOfUsers)
        {
            var users = Builder<UserDto>.CreateListOfSize(numberOfUsers)
                .All()
                    .With(e => e.Id = string.Empty)
                    .With(e => e.Name = Faker.Name.FullName())
                .Build().ToList();

            return users;
        }
    }
}
