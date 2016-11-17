namespace Perfectial.Domain.Model
{
    public class UserClaimBase<TPrimaryKey> : EntityBase<TPrimaryKey>
    {
        public virtual TPrimaryKey UserId { get; set; }

        public virtual string ClaimType { get; set; }
        public virtual string ClaimValue { get; set; }
    }
}
