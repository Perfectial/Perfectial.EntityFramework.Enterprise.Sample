namespace Perfectial.Domain.Model
{
    using System;
    using System.Collections.Generic;

    public class User : UserBase<string, UserLogin, UserRole, UserClaim>
    {
        public User()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public virtual ICollection<ToDoItem> ToDoItems { get; set; } = new List<ToDoItem>();
    }
}
