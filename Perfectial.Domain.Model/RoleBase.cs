namespace Perfectial.Domain.Model
{
    using System.Collections.Generic;

    public class RoleBase<TPrimaryKey, TUserRole> : EntityBase<TPrimaryKey> where TUserRole : UserRoleBase<TPrimaryKey>
    {
        public RoleBase()
        {
            this.UserRoles = new List<TUserRole>();
        }

        public string Name { get; set; }

        public virtual ICollection<TUserRole> UserRoles { get; private set; }
    }
}