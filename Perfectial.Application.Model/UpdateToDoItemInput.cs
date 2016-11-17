namespace Perfectial.Application.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UpdateToDoItemInput : IInputDto
    {
        [Range(1, int.MaxValue)]
        public int ToDoItemId { get; set; }

        public string AssignedUserId { get; set; }

        public ToDoItemState? State { get; set; }

        public override string ToString()
        {
            return $"[ToDoItemId = {0}, AssignedPersonId = {this.AssignedUserId}, State = {this.State}]";
        }
    }
}