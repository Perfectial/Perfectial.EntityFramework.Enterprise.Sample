namespace Perfectial.Domain.Model
{
    using System;
    using System.Collections.Generic;

    public class UserBase<TPrimaryKey, TUserLogin, TUserRole, TUserClaim> : EntityBase<TPrimaryKey>, IUser
        where TUserLogin : UserLoginBase<TPrimaryKey>
        where TUserRole : UserRoleBase<TPrimaryKey>
        where TUserClaim : UserClaimBase<TPrimaryKey>
    {
        public UserBase()
        {
            this.UserRoles = new List<TUserRole>();
            this.Claims = new List<TUserClaim>();
            this.Logins = new List<TUserLogin>();
        }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        /// <value> The user's name. </value>
        public virtual string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user's e-mail address.
        /// </summary>
        /// <value> The user's e-mail address. </value>
        public virtual string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email is confirmed.
        /// </summary>
        /// <value> True if the email is confirmed, default is false. </value>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the salted/hashed form of the user password.
        /// </summary>
        /// <value> The salted/hashed form of the user password. </value>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets a random value that should change whenever a users credentials have changed (password changed, login removed).
        /// </summary>
        /// <value> A random value that should change whenever a users credentials have changed (password changed, login removed). </value>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        /// <value> The user's phone number. </value>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's phone number is confirmed.
        /// </summary>
        /// <value> True if the user's phone number is confirmed, default is false. </value>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether two factor authentication enabled for the user.
        /// </summary>
        /// <value> A value indicating whether two factor authentication enabled for the user. </value>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Gets or sets the date time in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        /// <value> The date time in UTC when lockout ends, any time in the past is considered not locked out. </value>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lockout is enabled for this user.
        /// </summary>
        /// <value> A value indicating whether lockout is enabled for this user. </value>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that is used to record failures for the purposes of lockout.
        /// </summary>
        /// <value> A value that is used to record failures for the purposes of lockout. </value>
        public virtual int AccessFailedCount { get; set; }

        public virtual ICollection<TUserRole> UserRoles { get; private set; }
        public virtual ICollection<TUserClaim> Claims { get; private set; }
        public virtual ICollection<TUserLogin> Logins { get; private set; }
    }
}
