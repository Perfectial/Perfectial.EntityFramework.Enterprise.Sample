namespace Perfectial.Application.Services.Base
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public interface IUserIdentityApplicationService : IApplicationServiceBase
    {
        Task<IdentityResult> AddRoleAsync(string userId, string role);
        Task<IdentityResult> AddRolesAsync(string userId, params string[] roles);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task<IdentityResult> CreateAsync(User user);
        Task<IdentityResult> CreateAsync(User user, string password);
        Task<IdentityResult> CreateClaimAsync(string userId, Claim claim);
        Task<ClaimsIdentity> CreateClaimsIdentityAsync(User user, string authenticationType);
        Task<IdentityResult> CreateLoginAsync(string userId, UserLinkedLogin userLinkedLogin);
        Task<IdentityResult> DeleteAsync(User user);
        Task<User> FindByEmailAsync(string email);
        Task<User> FindByIdAsync(string userId);
        Task<User> FindByLoginAsync(UserLinkedLogin userLinkedLogin);
        Task<User> FindByNameAsync(string userName);
        Task<User> FindByNameAsync(string userName, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<string> GeneratePhoneNumberConfirmationTokenAsync(string userId, string phoneNumber);
        Task<string> GenerateResetPasswordUserTokenAsync(string userId);
        Task<string> GenerateTwoFactorUserTokenProviderAsync(string userId, string twoFactorProviderKey);
        Task<string> GenerateUserTokenAsync(string purpose, string userId);
        Task<int> GetAccessFailedAttemptsAsync(string userId);
        Task<IList<Claim>> GetClaimsAsync(string userId);
        Task<string> GetEmailAsync(string userId);
        Task<bool> GetLockoutEnabledAsync(string userId);
        Task<DateTimeOffset> GetLockoutEndDateAsync(string userId);
        Task<IList<UserLinkedLogin>> GetLoginsAsync(string userId);
        Task<string> GetPhoneNumberAsync(string userId);
        Task<IList<string>> GetRolesAsync(string userId);
        Task<string> GetSecurityStampAsync(string userId);
        Task<bool> GetTwoFactorEnabledAsync(string userId);
        Task<IList<string>> GetValidTwoFactorUserTokenProvidersAsync(string userId);
        Task<bool> HasPasswordAsync(string userId);
        Task<IdentityResult> IncrementAccessFailedAttemptsAsync(string userId);
        Task<bool> IsEmailConfirmedAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<bool> IsLockedOutAsync(string userId);
        Task<bool> IsPhoneNumberConfirmedAsync(string userId);
        Task NotifyTwoFactorUserTokenProviderAsync(string userId, string twoFactorProviderKey, string token);
        Task RegisterTwoFactorUserTokenProviderAsync(string twoFactorProviderKey, IUserTokenProvider provider);
        Task<IdentityResult> RemoveClaimAsync(string userId, Claim claim);
        Task<IdentityResult> RemoveLoginAsync(string userId, UserLinkedLogin userLinkedLogin);
        Task<IdentityResult> RemovePasswordAsync(string userId);
        Task<IdentityResult> RemoveRoleAsync(string userId, string role);
        Task<IdentityResult> RemoveRolesAsync(string userId, params string[] roles);
        Task<IdentityResult> ResetAccessFailedAttemptsAsync(string userId);
        Task<IdentityResult> ResetPasswordAsync(string userId, string userToken, string newPassword);
        Task<IdentityResult> SetEmailAsync(string userId, string email);
        Task<IdentityResult> SetLockoutEnabledAsync(string userId, bool lockoutIsEnabled);
        Task<IdentityResult> SetLockoutEndDateAsync(string userId, DateTimeOffset lockoutEndDate);
        Task<IdentityResult> SetPasswordAsync(string userId, string password);
        Task<IdentityResult> SetPhoneNumberAsync(string userId, string phoneNumber);
        Task<IdentityResult> SetTwoFactorEnabledAsync(string userId, bool twoFactorUserTokenAuthenticationIsEnabled);
        Task<IdentityResult> UpdateAsync(User user);
        Task<IdentityResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<IdentityResult> UpdatePhoneNumberAsync(string userId, string phoneNumber, string token);
        Task<IdentityResult> UpdateSecurityStampAsync(string userId);
        Task<bool> ValidateTwoFactorUserTokenProviderAsync(string userId, string twoFactorProviderKey, string token);
        Task<bool> ValidateUserTokenAsync(string userId, string modifier, string userToken);
    }
}
