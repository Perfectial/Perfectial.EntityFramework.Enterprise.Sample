namespace Perfectial.Application.Model
{
    using System;

    public class ToDoItemDto : EntityDto
    {
        public string AssignedUserId { get; set; }

        public string AssignedUserName { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public ToDoItemState State { get; set; }

        public override string ToString()
        {
            return $"[Task Id={this.Id}, Description={this.Description}, CreationTime={this.CreationTime}, AssignedPersonName={this.AssignedUserId}, State={this.State}]";
        }
    }
}