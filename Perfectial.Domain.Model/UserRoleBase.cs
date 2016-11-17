namespace Perfectial.Domain.Model
{
    public class UserRoleBase<TPrimaryKey>
    {
        public virtual TPrimaryKey UserId { get; set; }
        public virtual TPrimaryKey RoleId { get; set; }
    }
}
