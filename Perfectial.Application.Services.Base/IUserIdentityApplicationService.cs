namespace Perfectial.Application.Services.Base
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public interface IUserIdentityApplicationService : IApplicationServiceBase
    {
        Task<IdentityResult> CreateAsync(User user);
        Task<IdentityResult> CreateAsync(User user, string password);
        Task<User> FindByIdAsync(string userId);
        Task<User> FindByNameAsync(string userName);
        Task<User> FindByNameAsync(string userName, string password);
        Task<User> FindByLoginAsync(UserLinkedLogin userLinkedLogin);
        Task<User> FindByEmailAsync(string email);
        Task<IdentityResult> UpdateAsync(User user);
        Task<IdentityResult> DeleteAsync(User user);

        Task<IdentityResult> CreateRoleAsync(string userId, string role);
        Task<IdentityResult> CreateRolesAsync(string userId, params string[] roles);
        Task<IList<string>> GetRolesAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<IdentityResult> DeleteRoleAsync(string userId, string role);
        Task<IdentityResult> DeleteRolesAsync(string userId, params string[] roles);

        Task<IdentityResult> CreateClaimAsync(string userId, Claim claim);
        Task<IList<Claim>> GetClaimsAsync(string userId);
        Task<IdentityResult> DeleteClaimAsync(string userId, Claim claim);

        Task<IdentityResult> CreateLoginAsync(string userId, UserLinkedLogin userLinkedLogin);
        Task<IList<UserLinkedLogin>> GetLoginsAsync(string userId);
        Task<IdentityResult> DeleteLoginAsync(string userId, UserLinkedLogin userLinkedLogin);

        Task<bool> HasPasswordAsync(string userId);
        Task<IdentityResult> SetPasswordAsync(string userId, string password);
        Task<IdentityResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(string userId, string userToken, string newPassword);
        Task<IdentityResult> RemovePasswordAsync(string userId);

        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task<bool> IsEmailConfirmedAsync(string userId);
        Task<string> GetEmailAsync(string userId);
        Task<IdentityResult> SetEmailAsync(string userId, string email);

        Task<bool> IsPhoneNumberConfirmedAsync(string userId);
        Task<string> GetPhoneNumberAsync(string userId);
        Task<IdentityResult> SetPhoneNumberAsync(string userId, string phoneNumber);
        Task<IdentityResult> UpdatePhoneNumberAsync(string userId, string phoneNumber, string token);

        Task<bool> IsLockedOutAsync(string userId);
        Task<bool> GetLockoutEnabledAsync(string userId);
        Task<IdentityResult> SetLockoutEnabledAsync(string userId, bool lockoutIsEnabled);
        Task<DateTimeOffset> GetLockoutEndDateAsync(string userId);
        Task<IdentityResult> SetLockoutEndDateAsync(string userId, DateTimeOffset lockoutEndDate);

        Task<string> GetSecurityStampAsync(string userId);
        Task<IdentityResult> UpdateSecurityStampAsync(string userId);
        Task<int> GetAccessFailedAttemptsAsync(string userId);
        Task<IdentityResult> ResetAccessFailedAttemptsAsync(string userId);
        Task<IdentityResult> IncrementAccessFailedAttemptsAsync(string userId);

        Task<string> GenerateUserTokenAsync(string purpose, string userId);
        Task<bool> ValidateUserTokenAsync(string userId, string modifier, string userToken);

        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<string> GeneratePhoneNumberConfirmationTokenAsync(string userId, string phoneNumber);
        Task<string> GenerateResetPasswordUserTokenAsync(string userId);

        Task<bool> GetTwoFactorUserTokenAuthenticationIsEnabledAsync(string userId);
        Task<IdentityResult> SetTwoFactorUserTokenAuthenticationEnabledAsync(string userId, bool twoFactorUserTokenAuthenticationIsEnabled);

        Task<IList<string>> GetValidTwoFactorAuthenticationProvidersAsync(string userId);
        Task<string> GenerateTwoFactorAuthenticationUserTokenAsync(string userId, string twoFactorProviderKey);
        void RegisterTwoFactorAuthenticationProvider(string twoFactorProviderKey, IUserTokenProvider provider);
        Task NotifyTwoFactorAuthenticationProviderAsync(string userId, string twoFactorProviderKey, string token);
        Task<bool> ValidateTwoFactorAuthenticationProviderAsync(string userId, string twoFactorProviderKey, string token);

        Task<SignInStatus> SignInAsync(User user, bool isPersistentCookie, bool rememberBrowserCookie);

        Task<string> GetTwoFactorAuthenticationUserIdAsync();
        Task<bool> SendTwoFactorAuthenticationUserTokenAsync(string twoFactorProviderKey);
        Task<SignInStatus> SignInWithTwoFactorAuthenticationAsync(string twoFactorProviderKey, string token, bool isPersistentCookie, bool rememberBrowserCookie);

        Task<UserExternalLogin> GetUserExternalLoginAsync();
        Task<UserExternalLogin> GetUserExternalLoginAsync(string xsrfKey, string expectedValue);
        Task<SignInStatus> SignInWithUserExternalLoginAsync(UserExternalLogin userExternalLogin, bool isPersistentCookie);

        Task<SignInStatus> SignInWithTwoFactorCookieAsync(User user);
        Task<SignInStatus> SignInWithUsernameAndPasswordAsync(string userName, string password, bool isPersistentCookie, bool shouldLockout);

        void SignOut();

        Task<bool> AuthenticateIdentityAsync(string userId, string authenticationType);
        Task<ClaimsIdentity> CreateUserClaimsIdentityAsync(User user);

        IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes();
    }
}
