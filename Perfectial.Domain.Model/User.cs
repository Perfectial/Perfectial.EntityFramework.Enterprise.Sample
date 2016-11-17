namespace Perfectial.Domain.Model
{
    using System;

    public class User : UserBase<string, UserLogin, UserRole, UserClaim>
    {
        public User()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public User(string userName) : this()
        {
            this.UserName = userName;
        }
    }
}
