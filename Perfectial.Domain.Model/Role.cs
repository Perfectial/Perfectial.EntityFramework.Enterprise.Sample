namespace Perfectial.Domain.Model
{
    using System;

    public class Role : RoleBase<string, UserRole>
    {
        public Role()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public Role(string name) : this()
        {
            this.Name = name;
        }
    }
}