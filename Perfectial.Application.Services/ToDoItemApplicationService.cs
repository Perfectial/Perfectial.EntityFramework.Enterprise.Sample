namespace Perfectial.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using AutoMapper;

    using Common.Logging;

    using Perfectial.Application.Model;
    using Perfectial.Application.Services.Base;
    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    using ToDoItemState = Perfectial.Domain.Model.ToDoItemState;

    public class ToDoItemApplicationService : ApplicationServiceBase, IToDoItemApplicationService
    {
        private readonly IRepository<ToDoItem, int> toDoItemRepository;
        private readonly IRepository<User, string> userRepository;

        public ToDoItemApplicationService(
            IRepository<ToDoItem, int> toDoItemRepository, 
            IRepository<User, string> userRepository, 
            IDbContextScopeFactory dbContextScopeFactory, 
            IMapper mapper, 
            ILog logger)
            : base(dbContextScopeFactory, mapper, logger)
        {
            this.toDoItemRepository = toDoItemRepository;
            this.userRepository = userRepository;
        }

        public GetToDoItemsOutput GetToDoItems(GetToDoItemInput input)
        {
            this.Logger.Info($"Getting ToDoItems for input: {input}");

            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var query = this.toDoItemRepository.GetAll();

                if (!string.IsNullOrEmpty(input.AssignedUserId))
                {
                    query = query.Where(toDoItem => toDoItem.AssignedUserId == input.AssignedUserId);
                }

                if (input.State.HasValue)
                {
                    query = query.Where(toDoItem => toDoItem.State == this.Mapper.Map<ToDoItemState>(input.State.Value));
                }

                var toDoItems = query.OrderByDescending(toDoItem => toDoItem.CreationTime).Include(toDoItem => toDoItem.AssignedUser).ToList();
                var output = new GetToDoItemsOutput { ToDoItems = this.Mapper.Map<List<ToDoItemDto>>(toDoItems) };

                this.Logger.Info($"Got ToDoItems output: {output} for input: {input}");

                return output;
            }
        }

        public void UpdateToDoItem(UpdateToDoItemInput input)
        {
            this.Logger.Info($"Updating ToDoItem for input: {input}");

            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                var toDoItem = this.toDoItemRepository.Get(input.ToDoItemId);
                if (input.State.HasValue)
                {
                    toDoItem.State = this.Mapper.Map<ToDoItemState>(input.State.Value);
                }

                if (!string.IsNullOrEmpty(input.AssignedUserId))
                {
                    toDoItem.AssignedUser = this.userRepository.Get(input.AssignedUserId);
                }

                this.toDoItemRepository.Update(toDoItem);

                dbContextScope.SaveChanges();
            }

            this.Logger.Info($"Updated ToDoItem for input: {input}");
        }

        public void CreateToDoItem(CreateToDoItemInput input)
        {
            this.Logger.Info($"Creating ToDoItem for input: {input}");

            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                var toDoItem = new ToDoItem { Description = input.Description };
                if (!string.IsNullOrEmpty(input.AssignedUserId))
                {
                    toDoItem.AssignedUser = this.userRepository.Get(input.AssignedUserId);
                }

                this.toDoItemRepository.Add(toDoItem);

                dbContextScope.SaveChanges();
            }

            this.Logger.Info($"Created ToDoItem for input: {input}");
        }
    }
}