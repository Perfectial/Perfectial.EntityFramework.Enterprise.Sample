namespace Perfectial.Application.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CreateToDoItemInput : IInputDto
    {
        public string AssignedUserId { get; set; }

        [Required]
        public string Description { get; set; }

        public override string ToString()
        {
            return $"[AssignedPersonId = {this.AssignedUserId}, Description = {this.Description}]";
        }
    }
}