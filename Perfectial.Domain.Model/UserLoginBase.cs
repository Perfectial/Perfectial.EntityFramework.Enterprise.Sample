namespace Perfectial.Domain.Model
{
    public class UserLoginBase<TPrimaryKey>
    {
        public virtual string LoginProvider { get; set; }
        public virtual string ProviderKey { get; set; }

        public virtual TPrimaryKey UserId { get; set; }
    }
}
