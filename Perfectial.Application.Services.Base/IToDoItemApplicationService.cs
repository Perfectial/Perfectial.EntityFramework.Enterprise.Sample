namespace Perfectial.Application.Services.Base
{
    using Perfectial.Application.Model;

    public interface IToDoItemApplicationService : IApplicationServiceBase
    {
        GetToDoItemsOutput GetToDoItems(GetToDoItemInput input);
        void UpdateToDoItem(UpdateToDoItemInput input);
        void CreateToDoItem(CreateToDoItemInput input);
    }
}