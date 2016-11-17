namespace Perfectial.Application.Model
{
    using System.Collections.Generic;

    public class GetToDoItemsOutput : IInputDto
    {
        public List<ToDoItemDto> ToDoItems { get; set; }
    }
}