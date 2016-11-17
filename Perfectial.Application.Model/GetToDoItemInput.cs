namespace Perfectial.Application.Model
{
    using System;

    public class GetToDoItemInput : IInputDto
    {
        public ToDoItemState? State { get; set; }

        public string AssignedUserId { get; set; }
    }
}