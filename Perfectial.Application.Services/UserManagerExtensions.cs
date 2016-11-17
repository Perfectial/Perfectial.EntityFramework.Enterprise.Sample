namespace Perfectial.Infrastructure.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    using TaskExtensions = System.Data.Entity.SqlServer.Utilities.TaskExtensions;

    /// <summary>
    /// Extension methods for UserManager
    /// 
    /// </summary>
    public static class UserManagerExtensions
    {
        /*UserManager<TUser, TPrimaryKey> : IDisposable where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser*/
        /// <summary>
        /// Creates a ClaimsIdentity representing the user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="user"/><param name="authenticationType"/>
        /// <returns/>
        public static ClaimsIdentity CreateIdentity<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TUser user, string authenticationType) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<ClaimsIdentity>((Func<Task<ClaimsIdentity>>)(() => manager.CreateIdentityAsync(user, authenticationType)));
        }

        /// <summary>
        /// Find a user by id
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static TUser FindById<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<TUser>((Func<Task<TUser>>)(() => manager.FindByIdAsync(userId)));
        }

        /// <summary>
        /// Return a user with the specified username and password or null if there is no match.
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userName"/><param name="password"/>
        /// <returns/>
        public static TUser Find<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, string userName, string password) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<TUser>((Func<Task<TUser>>)(() => manager.FindAsync(userName, password)));
        }

        /// <summary>
        /// Find a user by name
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userName"/>
        /// <returns/>
        public static TUser FindByName<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, string userName) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<TUser>((Func<Task<TUser>>)(() => manager.FindByNameAsync(userName)));
        }

        /// <summary>
        /// Find a user by email
        /// 
        /// </summary>
        /// <param name="manager"/><param name="email"/>
        /// <returns/>
        public static TUser FindByEmail<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, string email) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<TUser>((Func<Task<TUser>>)(() => manager.FindByEmailAsync(email)));
        }

        /// <summary>
        /// Create a user with no password
        /// 
        /// </summary>
        /// <param name="manager"/><param name="user"/>
        /// <returns/>
        public static IdentityResult Create<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TUser user) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.CreateAsync(user)));
        }

        /// <summary>
        /// Create a user and associates it with the given password (if one is provided)
        /// 
        /// </summary>
        /// <param name="manager"/><param name="user"/><param name="password"/>
        /// <returns/>
        public static IdentityResult Create<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TUser user, string password) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.CreateAsync(user, password)));
        }

        /// <summary>
        /// Update an user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="user"/>
        /// <returns/>
        public static IdentityResult Update<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TUser user) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.UpdateAsync(user)));
        }

        /// <summary>
        /// Delete an user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="user"/>
        /// <returns/>
        public static IdentityResult Delete<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TUser user) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.DeleteAsync(user)));
        }

        /// <summary>
        /// Returns true if a user has a password set
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static bool HasPassword<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.HasPasswordAsync(userId)));
        }

        /// <summary>
        /// Add a user password only if one does not already exist
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="password"/>
        /// <returns/>
        public static IdentityResult AddPassword<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string password) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.AddPasswordAsync(userId, password)));
        }

        /// <summary>
        /// Change a user password
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="currentPassword"/><param name="newPassword"/>
        /// <returns/>
        public static IdentityResult ChangePassword<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string currentPassword, string newPassword) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.ChangePasswordAsync(userId, currentPassword, newPassword)));
        }

        /// <summary>
        /// Reset a user's password using a reset password token
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="token">This should be the user's security stamp by default</param><param name="newPassword"/>
        /// <returns/>
        public static IdentityResult ResetPassword<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string token, string newPassword) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.ResetPasswordAsync(userId, token, newPassword)));
        }

        /// <summary>
        /// Get the password reset token for the user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static string GeneratePasswordResetToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GeneratePasswordResetTokenAsync(userId)));
        }

        /// <summary>
        /// Get the current security stamp for a user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static string GetSecurityStamp<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GetSecurityStampAsync(userId)));
        }

        /// <summary>
        /// Get the confirmation token for the user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static string GenerateEmailConfirmationToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GenerateEmailConfirmationTokenAsync(userId)));
        }

        /// <summary>
        /// Confirm the user with confirmation token
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="token"/>
        /// <returns/>
        public static IdentityResult ConfirmEmail<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string token) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.ConfirmEmailAsync(userId, token)));
        }

        /// <summary>
        /// Returns true if the user's email has been confirmed
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static bool IsEmailConfirmed<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.IsEmailConfirmedAsync(userId)));
        }

        /// <summary>
        /// Generate a new security stamp for a user, used for SignOutEverywhere functionality
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IdentityResult UpdateSecurityStamp<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.UpdateSecurityStampAsync(userId)));
        }

        /// <summary>
        /// Returns true if the password combination is valid for the user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="user"/><param name="password"/>
        /// <returns/>
        public static bool CheckPassword<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TUser user, string password) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.CheckPasswordAsync(user, password)));
        }

        /// <summary>
        /// Associate a login with a user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IdentityResult RemovePassword<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.RemovePasswordAsync(userId)));
        }

        /// <summary>
        /// Sync extension
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="login"/>
        /// <returns/>
        public static IdentityResult AddLogin<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, UserLoginInfo login) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.AddLoginAsync(userId, login)));
        }

        /// <summary>
        /// Remove a user login
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="login"/>
        /// <returns/>
        public static IdentityResult RemoveLogin<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, UserLoginInfo login) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.RemoveLoginAsync(userId, login)));
        }

        /// <summary>
        /// Gets the logins for a user.
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IList<UserLoginInfo> GetLogins<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IList<UserLoginInfo>>((Func<Task<IList<UserLoginInfo>>>)(() => manager.GetLoginsAsync(userId)));
        }

        /// <summary>
        /// Sync extension
        /// 
        /// </summary>
        /// <param name="manager"/><param name="login"/>
        /// <returns/>
        public static TUser Find<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, UserLoginInfo login) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<TUser>((Func<Task<TUser>>)(() => manager.FindAsync(login)));
        }

        /// <summary>
        /// Add a user claim
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="claim"/>
        /// <returns/>
        public static IdentityResult AddClaim<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, Claim claim) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.AddClaimAsync(userId, claim)));
        }

        /// <summary>
        /// Remove a user claim
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="claim"/>
        /// <returns/>
        public static IdentityResult RemoveClaim<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, Claim claim) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.RemoveClaimAsync(userId, claim)));
        }

        /// <summary>
        /// Get a users's claims
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IList<Claim> GetClaims<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IList<Claim>>((Func<Task<IList<Claim>>>)(() => manager.GetClaimsAsync(userId)));
        }

        /// <summary>
        /// Add a user to a role
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="role"/>
        /// <returns/>
        public static IdentityResult AddToRole<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string role) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.AddToRoleAsync(userId, role)));
        }

        /// <summary>
        /// Add a user to several roles
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="roles"/>
        /// <returns/>
        public static IdentityResult AddToRoles<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, params string[] roles) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.AddToRolesAsync(userId, roles)));
        }

        /// <summary>
        /// Remove a user from a role.
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="role"/>
        /// <returns/>
        public static IdentityResult RemoveFromRole<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string role) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.RemoveFromRoleAsync(userId, role)));
        }

        /// <summary>
        /// Remove a user from the specified roles.
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="roles"/>
        /// <returns/>
        public static IdentityResult RemoveFromRoles<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, params string[] roles) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.RemoveFromRolesAsync(userId, roles)));
        }

        /// <summary>
        /// Get a users's roles
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IList<string> GetRoles<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IList<string>>((Func<Task<IList<string>>>)(() => manager.GetRolesAsync(userId)));
        }

        /// <summary>
        /// Returns true if the user is in the specified role
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="role"/>
        /// <returns/>
        public static bool IsInRole<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string role) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.IsInRoleAsync(userId, role)));
        }

        /// <summary>
        /// Get an user's email
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static string GetEmail<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GetEmailAsync(userId)));
        }

        /// <summary>
        /// Set an user's email
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="email"/>
        /// <returns/>
        public static IdentityResult SetEmail<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string email) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.SetEmailAsync(userId, email)));
        }

        /// <summary>
        /// Get an user's phoneNumber
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static string GetPhoneNumber<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GetPhoneNumberAsync(userId)));
        }

        /// <summary>
        /// Set an user's phoneNumber
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="phoneNumber"/>
        /// <returns/>
        public static IdentityResult SetPhoneNumber<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string phoneNumber) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.SetPhoneNumberAsync(userId, phoneNumber)));
        }

        /// <summary>
        /// Change a phone number using the verification token
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="phoneNumber"/><param name="token"/>
        /// <returns/>
        public static IdentityResult ChangePhoneNumber<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string phoneNumber, string token) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.ChangePhoneNumberAsync(userId, phoneNumber, token)));
        }

        /// <summary>
        /// Generate a token for using to change to a specific phone number for the user
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="phoneNumber"/>
        /// <returns/>
        public static string GenerateChangePhoneNumberToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string phoneNumber) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GenerateChangePhoneNumberTokenAsync(userId, phoneNumber)));
        }

        /// <summary>
        /// Verify that a token is valid for changing the user's phone number
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="token"/><param name="phoneNumber"/>
        /// <returns/>
        public static bool VerifyChangePhoneNumberToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string token, string phoneNumber) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.VerifyChangePhoneNumberTokenAsync(userId, token, phoneNumber)));
        }

        /// <summary>
        /// Returns true if the user's phone number has been confirmed
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static bool IsPhoneNumberConfirmed<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.IsPhoneNumberConfirmedAsync(userId)));
        }

        /// <summary>
        /// Get a user token for a factor provider
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="providerId"/>
        /// <returns/>
        public static string GenerateTwoFactorToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string providerId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GenerateTwoFactorTokenAsync(userId, providerId)));
        }

        /// <summary>
        /// Verify a user factor token with the specified provider
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="providerId"/><param name="token"/>
        /// <returns/>
        public static bool VerifyTwoFactorToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string providerId, string token) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.VerifyTwoFactorTokenAsync(userId, providerId, token)));
        }

        /// <summary>
        /// Returns a list of valid two factor providers for a user
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IList<string> GetValidTwoFactorProviders<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IList<string>>((Func<Task<IList<string>>>)(() => manager.GetValidTwoFactorProvidersAsync(userId)));
        }

        /// <summary>
        /// Get a user token for a specific purpose
        /// 
        /// </summary>
        /// <param name="manager"/><param name="purpose"/><param name="userId"/>
        /// <returns/>
        public static string GenerateUserToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, string purpose, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<string>((Func<Task<string>>)(() => manager.GenerateUserTokenAsync(purpose, userId)));
        }

        /// <summary>
        /// Validate a user token
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="purpose"/><param name="token"/>
        /// <returns/>
        public static bool VerifyUserToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string purpose, string token) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.VerifyUserTokenAsync(userId, purpose, token)));
        }

        /// <summary>
        /// Notify a user with a token from a specific user factor provider
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="twoFactorProvider"/><param name="token"/>
        /// <returns/>
        public static IdentityResult NotifyTwoFactorToken<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string twoFactorProvider, string token) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.NotifyTwoFactorTokenAsync(userId, twoFactorProvider, token)));
        }

        /// <summary>
        /// Returns true if two factor is enabled for the user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static bool GetTwoFactorEnabled<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.GetTwoFactorEnabledAsync(userId)));
        }

        /// <summary>
        /// Set whether a user's two factor is enabled
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="enabled"/>
        /// <returns/>
        public static IdentityResult SetTwoFactorEnabled<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, bool enabled) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.SetTwoFactorEnabledAsync(userId, enabled)));
        }

        /// <summary>
        /// Send email with supplied subject and body
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="subject"/><param name="body"/>
        public static void SendEmail<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string subject, string body) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            AsyncHelper.RunSync((Func<Task>)(() => manager.SendEmailAsync(userId, subject, body)));
        }

        /// <summary>
        /// Send text message using the given message
        /// 
        /// </summary>
        /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/><param name="manager"/><param name="userId"/><param name="message"/>
        public static void SendSms<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, string message) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            AsyncHelper.RunSync((Func<Task>)(() => manager.SendSmsAsync(userId, message)));
        }

        /// <summary>
        /// Returns true if the user is locked out
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static bool IsLockedOut<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.IsLockedOutAsync(userId)));
        }

        /// <summary>
        /// Sets whether the user allows lockout
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="enabled"/>
        /// <returns/>
        public static IdentityResult SetLockoutEnabled<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, bool enabled) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.SetLockoutEnabledAsync(userId, enabled)));
        }

        /// <summary>
        /// Returns whether the user allows lockout
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static bool GetLockoutEnabled<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<bool>((Func<Task<bool>>)(() => manager.GetLockoutEnabledAsync(userId)));
        }

        /// <summary>
        /// Returns the user lockout end date
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static DateTimeOffset GetLockoutEndDate<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<DateTimeOffset>((Func<Task<DateTimeOffset>>)(() => manager.GetLockoutEndDateAsync(userId)));
        }

        /// <summary>
        /// Sets the user lockout end date
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/><param name="lockoutEnd"/>
        /// <returns/>
        public static IdentityResult SetLockoutEndDate<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId, DateTimeOffset lockoutEnd) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.SetLockoutEndDateAsync(userId, lockoutEnd)));
        }

        /// <summary>
        /// Increments the access failed count for the user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IdentityResult AccessFailed<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.AccessFailedAsync(userId)));
        }

        /// <summary>
        /// Resets the access failed count for the user to 0
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static IdentityResult ResetAccessFailedCount<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<IdentityResult>((Func<Task<IdentityResult>>)(() => manager.ResetAccessFailedCountAsync(userId)));
        }

        /// <summary>
        /// Returns the number of failed access attempts for the user
        /// 
        /// </summary>
        /// <param name="manager"/><param name="userId"/>
        /// <returns/>
        public static int GetAccessFailedCount<TUser, TPrimaryKey>(this UserManager<TUser, TPrimaryKey> manager, TPrimaryKey userId) where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            return AsyncHelper.RunSync<int>((Func<Task<int>>)(() => manager.GetAccessFailedCountAsync(userId)));
        }
    }
}