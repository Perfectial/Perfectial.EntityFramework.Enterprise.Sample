namespace Perfectial.UnitTests.Application
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using AutoMapper.Internal;

    using FizzWare.NBuilder;

    using NUnit.Framework;

    using Perfectial.Application.Model;
    using Perfectial.Application.Services;
    using Perfectial.Domain.Model;

    using Rhino.Mocks;
    using Is = Rhino.Mocks.Constraints.Is;
    using ToDoItemState = Perfectial.Domain.Model.ToDoItemState;

    [TestFixture]
    public class ToDoItemApplicationServiceTest : ApplicationServiceTestBase
    {
        private const int NumberOfToDoItemsToCreate = 11;

        private readonly RandomGenerator enumGenerator = new RandomGenerator();

        [Test]
        public void ShouldCreateToDoItem()
        {
            var createToDoItemInput = new CreateToDoItemInput { AssignedUserId = Guid.NewGuid().ToString(), Description = Faker.Lorem.Sentence() };

            using (this.Repository.Record())
            {
                Expect.Call(this.DbContextScopeFactory.Create());
                Expect.Call(this.UserRepository.Get(string.Empty)).Constraints(Is.Equal(createToDoItemInput.AssignedUserId)).Repeat.Once();
                Expect.Call(this.ToDoItemRepository.Add(null)).Constraints(Is.Anything()).Repeat.Once();
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Creating ToDoItem for input:"))).Repeat.Once();
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Created ToDoItem for input:"))).Repeat.Once();
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.CreateToDoItem(createToDoItemInput);
            }
        }

        [Test]
        public void ShouldCreateToDoItemWithEmptyUser()
        {
            var createToDoItemInput = new CreateToDoItemInput { AssignedUserId = null, Description = Faker.Lorem.Sentence() };

            using (this.Repository.Record())
            {
                Expect.Call(this.DbContextScopeFactory.Create());
                Expect.Call(this.UserRepository.Get(string.Empty)).Constraints(Is.Equal(null)).Repeat.Never();
                Expect.Call(this.ToDoItemRepository.Add(null)).Constraints(Is.Anything()).Repeat.Once();
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Creating ToDoItem for input:"))).Repeat.Once();
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Created ToDoItem for input:"))).Repeat.Once();
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.CreateToDoItem(createToDoItemInput);
            }
        }

        [Test]
        public void ShouldUpdateToDoItem()
        {
            var updateToDoItemInput = new UpdateToDoItemInput
            {
                AssignedUserId = Guid.NewGuid().ToString(),
                State = (Perfectial.Application.Model.ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(Perfectial.Application.Model.ToDoItemState)).Length)
            };
            var updatedToDoItem = this.CreateToDoItem();

            var state = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length);
            var assignedUser = this.CreateUser();

            using (this.Repository.Record())
            {
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Updating ToDoItem for input:")));
                Expect.Call(this.DbContextScopeFactory.Create());
                Expect.Call(this.ToDoItemRepository.Get(0)).Constraints(Is.Equal(updateToDoItemInput.ToDoItemId)).Return(updatedToDoItem);
                Expect.Call(this.UserRepository.Get(string.Empty)).Constraints(Is.Equal(updateToDoItemInput.AssignedUserId)).Return(assignedUser);
                Expect.Call(this.Mapper.Map<ToDoItemState>(null)).Constraints(Is.Equal(updateToDoItemInput.State.Value)).Return(state);
                Expect.Call(this.ToDoItemRepository.Update(null)).Constraints(Is.Equal(updatedToDoItem)).Return(updatedToDoItem);
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Updated ToDoItem for input:")));
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.UpdateToDoItem(updateToDoItemInput);

                Assert.AreEqual(assignedUser, updatedToDoItem.AssignedUser);
                Assert.AreEqual(state, updatedToDoItem.State);
            }
        }

        [Test]
        public void ShouldUpdateToDoItemWithoutUser()
        {
            var updateToDoItemInput = new UpdateToDoItemInput
            {
                AssignedUserId = null,
                State = (Perfectial.Application.Model.ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(Perfectial.Application.Model.ToDoItemState)).Length)
            };
            var updatedToDoItem = this.CreateToDoItem();

            var state = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length);

            using (this.Repository.Record())
            {
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Updating ToDoItem for input:")));
                Expect.Call(this.DbContextScopeFactory.Create());
                Expect.Call(this.ToDoItemRepository.Get(0)).Constraints(Is.Equal(updateToDoItemInput.ToDoItemId)).Return(updatedToDoItem);
                Expect.Call(this.UserRepository.Get(string.Empty)).Constraints(Is.Anything()).Repeat.Never();
                Expect.Call(this.Mapper.Map<ToDoItemState>(null)).Constraints(Is.Equal(updateToDoItemInput.State.Value)).Return(state);
                Expect.Call(this.ToDoItemRepository.Update(null)).Constraints(Is.Equal(updatedToDoItem)).Return(updatedToDoItem);
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Updated ToDoItem for input:")));
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.UpdateToDoItem(updateToDoItemInput);

                Assert.AreEqual(state, updatedToDoItem.State);
            }
        }

        [Test]
        public void ShouldUpdateToDoItemWithoutState()
        {
            var updateToDoItemInput = new UpdateToDoItemInput
            {
                AssignedUserId = Guid.NewGuid().ToString(),
                State = null
            };
            var updatedToDoItem = this.CreateToDoItem();

            var assignedUser = this.CreateUser();

            using (this.Repository.Record())
            {
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Updating ToDoItem for input:")));
                Expect.Call(this.DbContextScopeFactory.Create());
                Expect.Call(this.ToDoItemRepository.Get(0)).Constraints(Is.Equal(updateToDoItemInput.ToDoItemId)).Return(updatedToDoItem);
                Expect.Call(this.UserRepository.Get(string.Empty)).Constraints(Is.Equal(updateToDoItemInput.AssignedUserId)).Return(assignedUser);
                Expect.Call(this.Mapper.Map<ToDoItemState>(null)).Constraints(Is.Anything()).Repeat.Never();
                Expect.Call(this.ToDoItemRepository.Update(null)).Constraints(Is.Equal(updatedToDoItem)).Return(updatedToDoItem);
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Updated ToDoItem for input:")));
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.UpdateToDoItem(updateToDoItemInput);

                Assert.AreEqual(assignedUser, updatedToDoItem.AssignedUser);
            }
        }

        [Test]
        public void ShouldGetToDoItemsWithUserAndState()
        {
            var getToDoItemInput = new GetToDoItemInput
            {
                AssignedUserId = Guid.NewGuid().ToString(),
                State = (Perfectial.Application.Model.ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(Perfectial.Application.Model.ToDoItemState)).Length)
            };

            Assert.IsNotNull(getToDoItemInput.State);
            var toDoItemState = (ToDoItemState)getToDoItemInput.State;

            var index = 0;
            var toDoItems = this.CreateToDoItems(NumberOfToDoItemsToCreate).AsQueryable();
            toDoItems.ToList().ForEach(
                toDoItem =>
                    {
                        if (index++ % 2 == 1)
                        {
                            toDoItem.AssignedUserId = getToDoItemInput.AssignedUserId;
                            toDoItem.State = toDoItemState;
                        }
                        else
                        {
                            while (toDoItem.State == toDoItemState)
                            {
                                toDoItem.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length);
                            }
                        }
                    });

            using (this.Repository.Record())
            {
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Getting ToDoItems for input:")));
                Expect.Call(this.DbContextScopeFactory.CreateReadOnly());
                Expect.Call(this.ToDoItemRepository.GetAll()).Return(toDoItems);
                Expect.Call(this.Mapper.Map<ToDoItemState>(null)).Constraints(Is.Equal(getToDoItemInput.State)).Return(toDoItemState).Repeat.AtLeastOnce();
                Expect.Call(this.Mapper.Map<List<ToDoItemDto>>(Arg<List<ToDoItem>>.List.Count(Is.Equal(toDoItems.Count() / 2))));
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Got ToDoItems output:")));
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.GetToDoItems(getToDoItemInput);
            }
        }

        [Test]
        public void ShouldGetToDoItemsWithUserAndWithoutState()
        {
            var getToDoItemInput = new GetToDoItemInput
            {
                AssignedUserId = Guid.NewGuid().ToString(),
                State = null
            };

            var index = 0;
            var toDoItems = this.CreateToDoItems(NumberOfToDoItemsToCreate).AsQueryable();
            toDoItems.ToList().ForEach(
                toDoItem =>
                {
                    if (index++ % 2 == 1)
                    {
                        toDoItem.AssignedUserId = getToDoItemInput.AssignedUserId;
                    }
                });

            using (this.Repository.Record())
            {
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Getting ToDoItems for input:")));
                Expect.Call(this.DbContextScopeFactory.CreateReadOnly());
                Expect.Call(this.ToDoItemRepository.GetAll()).Return(toDoItems);
                Expect.Call(this.Mapper.Map<ToDoItemState>(null)).Constraints(Is.Anything()).Repeat.Never();
                Expect.Call(this.Mapper.Map<List<ToDoItemDto>>(Arg<List<ToDoItem>>.List.Count(Is.Equal(toDoItems.Count() / 2))));
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Got ToDoItems output:")));
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.GetToDoItems(getToDoItemInput);
            }
        }

        [Test]
        public void ShouldGetToDoItemsWithoutUserAndWithState()
        {
            var getToDoItemInput = new GetToDoItemInput
            {
                AssignedUserId = null,
                State = (Perfectial.Application.Model.ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(Perfectial.Application.Model.ToDoItemState)).Length)
            };

            Assert.IsNotNull(getToDoItemInput.State);
            var toDoItemState = (ToDoItemState)getToDoItemInput.State;

            var index = 0;
            var toDoItems = this.CreateToDoItems(NumberOfToDoItemsToCreate).AsQueryable();
            toDoItems.ToList().ForEach(
                toDoItem =>
                {
                    toDoItem.AssignedUserId = Guid.NewGuid().ToString();

                    if (index++ % 2 == 1)
                    {
                        toDoItem.State = toDoItemState;
                    }
                    else
                    {
                        while (toDoItem.State == toDoItemState)
                        {
                            toDoItem.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length);
                        }
                    }
                });

            using (this.Repository.Record())
            {
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Getting ToDoItems for input:")));
                Expect.Call(this.DbContextScopeFactory.CreateReadOnly());
                Expect.Call(this.ToDoItemRepository.GetAll()).Return(toDoItems);
                Expect.Call(this.Mapper.Map<ToDoItemState>(null)).Constraints(Is.Equal(getToDoItemInput.State)).Return(toDoItemState).Repeat.AtLeastOnce();
                Expect.Call(this.Mapper.Map<List<ToDoItemDto>>(Arg<List<ToDoItem>>.List.Count(Is.Equal(toDoItems.Count() / 2))));
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Got ToDoItems output:")));
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                toDoItemApplicationService.GetToDoItems(getToDoItemInput);
            }
        }

        [Test]
        public void ShouldGetToDoItemsWithoutUserAndState()
        {
            var getToDoItemInput = new GetToDoItemInput
            {
                AssignedUserId = null,
                State = null
            };

            var toDoItems = this.CreateToDoItems(NumberOfToDoItemsToCreate).AsQueryable();
            toDoItems.ToList().ForEach(
                toDoItem =>
                {
                    toDoItem.AssignedUserId = Guid.NewGuid().ToString();
                    toDoItem.State = (ToDoItemState)this.enumGenerator.Next(0, Enum.GetValues(typeof(ToDoItemState)).Length);
                });

            using (this.Repository.Record())
            {
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Getting ToDoItems for input:")));
                Expect.Call(this.DbContextScopeFactory.CreateReadOnly());
                Expect.Call(this.ToDoItemRepository.GetAll()).Return(toDoItems);
                Expect.Call(this.Mapper.Map<ToDoItemState>(null)).Constraints(Is.Anything()).Repeat.Never();
                Expect.Call(this.Mapper.Map<List<ToDoItemDto>>(Arg<List<ToDoItem>>.List.Count(Is.Equal(toDoItems.Count()))));
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Got ToDoItems output:")));
            }

            using (this.Repository.Playback())
            {
                var toDoItemApplicationService = new ToDoItemApplicationService(
                    this.ToDoItemRepository,
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);

                toDoItemApplicationService.GetToDoItems(getToDoItemInput);
            }
        }
    }
}