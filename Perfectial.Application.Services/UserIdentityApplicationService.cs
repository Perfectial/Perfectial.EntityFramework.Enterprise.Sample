namespace Perfectial.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using AutoMapper;

    using Common.Logging;

    using Perfectial.Application.Services.Base;
    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    public class UserIdentityApplicationService : ApplicationServiceBase, IUserIdentityApplicationService
    {
        private const string ResetPasswordUserTokenModifier = "ResetPassword";
        private const string EmailConfirmationUserTokenModifier = "EmailConfirmation";
        private const string PhoneNumberConfirmationUserTokenModifier = "PhoneNumberConfirmation";

        private readonly IClaimsIdentityFactory claimsIdentityFactory;
        private readonly IPasswordHasher passwordHasher;
        private readonly IIdentityValidator<string> passwordIdentityValidator;
        private readonly IIdentityValidator<User> userIdentityValidator;
        private readonly IUserRepository userRepository;
        private readonly IUserTokenProvider userTokenProvider;
        private readonly IUserTokenTwoFactorProvider userTokenTwoFactorProvider;

        public UserIdentityApplicationService(
            IClaimsIdentityFactory claimsIdentityFactory,
            IPasswordHasher passwordHasher,
            IIdentityValidator<string> passwordIdentityValidator,
            IIdentityValidator<User> userIdentityValidator,
            IUserRepository userRepository,
            IDbContextScopeFactory dbContextScopeFactory,
            IUserTokenProvider userTokenProvider,
            IUserTokenTwoFactorProvider userTokenTwoFactorProvider,
            IMapper mapper,
            ILog logger)
            : base(dbContextScopeFactory, mapper, logger)
        {
            this.claimsIdentityFactory = claimsIdentityFactory;
            this.passwordHasher = passwordHasher;
            this.passwordIdentityValidator = passwordIdentityValidator;
            this.userIdentityValidator = userIdentityValidator;
            this.userRepository = userRepository;
            this.userTokenProvider = userTokenProvider;
            this.userTokenTwoFactorProvider = userTokenTwoFactorProvider;
        }

        public bool UserLockoutEnabledByDefault { get; set; }
        public int MaxFailedAccessAttemptsBeforeLockout { get; set; }
        public TimeSpan DefaultAccountLockoutTimeSpan { get; set; } = TimeSpan.Zero;

        public virtual async Task<ClaimsIdentity> CreateClaimsIdentityAsync(User user, string authenticationType)
        {
            var userRoles = await this.userRepository.GetRolesAsync(user);
            var userClaims = await this.userRepository.GetClaimsAsync(user);
            var claimsIdentity = await this.claimsIdentityFactory.CreateAsync(user, authenticationType, userRoles, userClaims);

            return claimsIdentity;
        }

        public virtual async Task<IdentityResult> CreateAsync(User user)
        {
            IdentityResult identityResult = await this.userIdentityValidator.ValidateAsync(user);
            if (identityResult.IsValid)
            {
                user.SecurityStamp = this.CreateSecurityStamp();
                if (this.UserLockoutEnabledByDefault)
                {
                    user.LockoutEnabled = true;
                }

                await this.userRepository.AddAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> CreateAsync(User user, string password)
        {
            IdentityResult identityResult = await this.UpdatePasswordAsync(user, password);
            if (identityResult.IsValid)
            {
                identityResult = await this.CreateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> UpdateAsync(User user)
        {
            IdentityResult identityResult = await this.userIdentityValidator.ValidateAsync(user);
            if (identityResult.IsValid)
            {
                await this.userRepository.UpdateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> DeleteAsync(User user)
        {
            await this.userRepository.DeleteAsync(user);

            IdentityResult identityResult = new IdentityResult(true, null);

            return identityResult;
        }

        public virtual async Task<User> FindByIdAsync(string userId)
        {
            var user = await this.userRepository.FindByIdAsync(userId);

            return user;
        }

        public virtual async Task<User> FindByNameAsync(string userName)
        {
            var user = await this.userRepository.FindByNameAsync(userName);

            return user;
        }

        public virtual async Task<User> FindByNameAsync(string userName, string password)
        {
            User user = await this.FindByNameAsync(userName);
            if (user != null)
            {
                if (!this.VerifyHashedPassword(user, password))
                {
                    user = null;
                }
            }

            return user;
        }

        public virtual async Task<bool> HasPasswordAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var userHasPassword = !string.IsNullOrEmpty(user.PasswordHash);

            return userHasPassword;
        }

        public virtual async Task<IdentityResult> SetPasswordAsync(string userId, string password)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                identityResult = await this.UpdatePasswordAsync(user, password);
                if (identityResult.IsValid)
                {
                    identityResult = await this.UpdateAsync(user);
                }
            }
            else
            {
                identityResult = new IdentityResult(false, new[] { Resource.UserAlreadyHasPassword });
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (this.VerifyHashedPassword(user, currentPassword))
            {
                identityResult = await this.UpdatePasswordAsync(user, newPassword);
                if (identityResult.IsValid)
                {
                    identityResult = await this.UpdateAsync(user);
                }
            }
            else
            {
                identityResult = new IdentityResult(false, new[] { Resource.PasswordMismatch });
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> RemovePasswordAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            user.PasswordHash = null;
            user.SecurityStamp = this.CreateSecurityStamp();

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<IdentityResult> ResetPasswordAsync(string userId, string userToken, string newPassword)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (await this.ValidateUserTokenAsync(userId, ResetPasswordUserTokenModifier, userToken))
            {
                identityResult = await this.UpdatePasswordAsync(user, newPassword);
                if (identityResult.IsValid)
                {
                    identityResult = await this.UpdateAsync(user);
                }
            }
            else
            {
                identityResult = new IdentityResult(false, new[] { Resource.InvalidToken });
            }

            return identityResult;
        }

        public virtual async Task<string> GetSecurityStampAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var securityStamp = user.SecurityStamp;

            return securityStamp;
        }

        public virtual async Task<IdentityResult> UpdateSecurityStampAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            user.SecurityStamp = this.CreateSecurityStamp();

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual Task<string> GenerateResetPasswordUserTokenAsync(string userId)
        {
            var resetPasswordUserToken = this.GenerateUserTokenAsync(ResetPasswordUserTokenModifier, userId);

            return resetPasswordUserToken;
        }

        public virtual async Task<IList<UserLinkedLogin>> GetLoginsAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var userLogins = await this.userRepository.GetLoginsAsync(user);

            return userLogins;
        }

        public virtual async Task<IdentityResult> CreateLoginAsync(string userId, UserLinkedLogin userLinkedLogin)
        {
            User userFoundById = await this.FindByIdAsync(userId);
            if (userFoundById == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;

            User userFoundByLogin = await this.FindByLoginAsync(userLinkedLogin);
            if (userFoundByLogin != null)
            {
                identityResult = new IdentityResult(false, new[] { Resource.ExternalLoginExists });
            }
            else
            {
                await this.userRepository.AddLoginAsync(userFoundById, userLinkedLogin);
                identityResult = await this.UpdateAsync(userFoundById);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> RemoveLoginAsync(string userId, UserLinkedLogin userLinkedLogin)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            await this.userRepository.RemoveLoginAsync(user, userLinkedLogin);
            user.SecurityStamp = this.CreateSecurityStamp();

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<User> FindByLoginAsync(UserLinkedLogin userLinkedLogin)
        {
            var user = await this.userRepository.FindByLoginAsync(userLinkedLogin);

            return user;
        }

        public virtual async Task<IList<Claim>> GetClaimsAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var userClaims = await this.userRepository.GetClaimsAsync(user);

            return userClaims;
        }

        public virtual async Task<IdentityResult> CreateClaimAsync(string userId, Claim claim)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            await this.userRepository.AddClaimAsync(user, claim);

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<IdentityResult> RemoveClaimAsync(string userId, Claim claim)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            await this.userRepository.RemoveClaimAsync(user, claim);

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<IList<string>> GetRolesAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var userRoles = await this.userRepository.GetRolesAsync(user);

            return userRoles;
        }

        public virtual async Task<IdentityResult> AddRoleAsync(string userId, string role)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;

            var userRoles = await this.userRepository.GetRolesAsync(user);
            if (userRoles.Contains(role))
            {
                identityResult = new IdentityResult(false, new[] { Resource.UserAlreadyInRole });
            }
            else
            {
                await this.userRepository.AddToRoleAsync(user, role);
                identityResult = await this.UpdateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> AddRolesAsync(string userId, params string[] roles)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;

            var userRoles = await this.userRepository.GetRolesAsync(user);
            if (roles.Any(userRole => userRoles.Contains(userRole)))
            {
                identityResult = new IdentityResult(false, new[] { Resource.UserAlreadyInRole });
            }
            else
            {
                foreach (var userRole in roles)
                {
                    await this.userRepository.AddToRoleAsync(user, userRole);
                }

                identityResult = await this.UpdateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> RemoveRoleAsync(string userId, string role)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (!await this.userRepository.IsInRoleAsync(user, role))
            {
                identityResult = new IdentityResult(false, new[] { Resource.UserNotInRole });
            }
            else
            {
                await this.userRepository.RemoveFromRoleAsync(user, role);
                identityResult = await this.UpdateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> RemoveRolesAsync(string userId, params string[] roles)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;

            var userRoles = await this.userRepository.GetRolesAsync(user);
            if (!roles.All(userRole => userRoles.Contains(userRole)))
            {
                identityResult = new IdentityResult(false, new[] { Resource.UserNotInRole });
            }
            else
            {
                foreach (var userRole in roles)
                {
                    await this.userRepository.RemoveFromRoleAsync(user, userRole);
                }

                identityResult = await this.UpdateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<bool> IsInRoleAsync(string userId, string role)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var isUserInRole = await this.userRepository.IsInRoleAsync(user, role);

            return isUserInRole;
        }

        public virtual async Task<string> GetEmailAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var email = user.Email;

            return email;
        }

        public virtual async Task<IdentityResult> SetEmailAsync(string userId, string email)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            user.Email = email;
            user.EmailConfirmed = false;
            user.SecurityStamp = this.CreateSecurityStamp();

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual Task<User> FindByEmailAsync(string email)
        {
            var user = this.userRepository.FindByEmailAsync(email);

            return user;
        }

        public virtual Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var emailConfirmationToken = this.GenerateUserTokenAsync(EmailConfirmationUserTokenModifier, userId);

            return emailConfirmationToken;
        }

        public virtual async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (await this.ValidateUserTokenAsync(userId, EmailConfirmationUserTokenModifier, token))
            {
                user.EmailConfirmed = true;
                identityResult = await this.UpdateAsync(user);
            }
            else
            {
                identityResult = new IdentityResult(false, new[] { Resource.InvalidToken });
            }

            return identityResult;
        }

        public virtual async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var emailIsConfirmed = user.EmailConfirmed;

            return emailIsConfirmed;
        }

        public virtual async Task<string> GetPhoneNumberAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var phoneNumber = user.PhoneNumber;

            return phoneNumber;
        }

        public virtual async Task<IdentityResult> SetPhoneNumberAsync(string userId, string phoneNumber)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            user.PhoneNumber = phoneNumber;
            user.EmailConfirmed = false;
            user.SecurityStamp = this.CreateSecurityStamp();

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<IdentityResult> UpdatePhoneNumberAsync(string userId, string phoneNumber, string token)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (await this.ValidateUserTokenAsync(userId, phoneNumber, token))
            {
                user.PhoneNumber = phoneNumber;
                user.PhoneNumberConfirmed = true;
                user.SecurityStamp = this.CreateSecurityStamp();

                identityResult = await this.UpdateAsync(user);
            }
            else
            {
                identityResult = new IdentityResult(false, new[] { Resource.InvalidToken });
            }

            return identityResult;
        }

        public virtual async Task<bool> IsPhoneNumberConfirmedAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var phoneNumberConfirmed = user.PhoneNumberConfirmed;

            return phoneNumberConfirmed;
        }

        public virtual Task<string> GeneratePhoneNumberConfirmationTokenAsync(string userId, string phoneNumber)
        {
            var phoneNumberConfirmationToken = this.GenerateUserTokenAsync(PhoneNumberConfirmationUserTokenModifier, userId);

            return phoneNumberConfirmationToken;
        }

        public virtual async Task RegisterTwoFactorUserTokenProviderAsync(string twoFactorProviderKey, IUserTokenProvider provider)
        {
            await this.userTokenTwoFactorProvider.RegisterTwoFactorUserTokenProviderAsync(twoFactorProviderKey, provider);
        }

        public virtual async Task<IList<string>> GetValidTwoFactorUserTokenProvidersAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var validTwoFactorUserTokenProviders = await this.userTokenTwoFactorProvider.GetValidTwoFactorUserTokenProvidersAsync(user);

            return validTwoFactorUserTokenProviders;
        }

        public virtual async Task<string> GenerateTwoFactorUserTokenProviderAsync(string userId, string twoFactorProviderKey)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var twoFactorUserToken = await this.userTokenTwoFactorProvider.GenerateTwoFactorUserTokenAsync(twoFactorProviderKey, user);

            return twoFactorUserToken;
        }

        public virtual async Task<bool> ValidateTwoFactorUserTokenProviderAsync(string userId, string twoFactorProviderKey, string token)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var twoFactorUserTokenIsValid = await this.userTokenTwoFactorProvider.ValidateTwoFactorUserTokenAsync(twoFactorProviderKey, user, token);

            return twoFactorUserTokenIsValid;
        }

        public virtual async Task NotifyTwoFactorUserTokenProviderAsync(string userId, string twoFactorProviderKey, string token)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            await this.userTokenTwoFactorProvider.NotifyTwoFactorUserTokenAsync(twoFactorProviderKey, user, token);
        }

        public virtual async Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var twoFactorUserTokenAuthenticationIsEnabled = user.TwoFactorEnabled;

            return twoFactorUserTokenAuthenticationIsEnabled;
        }

        public virtual async Task<IdentityResult> SetTwoFactorEnabledAsync(string userId, bool twoFactorUserTokenAuthenticationIsEnabled)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            user.TwoFactorEnabled = twoFactorUserTokenAuthenticationIsEnabled;
            user.SecurityStamp = this.CreateSecurityStamp();

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<bool> IsLockedOutAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            bool userIsLockedOut;
            if (!user.LockoutEnabled)
            {
                userIsLockedOut = false;
            }
            else
            {
                userIsLockedOut = user.LockoutEndDateUtc >= DateTimeOffset.UtcNow;
            }

            return userIsLockedOut;
        }

        public virtual async Task<bool> GetLockoutEnabledAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var lockoutIsEnabled = user.LockoutEnabled;

            return lockoutIsEnabled;
        }

        public virtual async Task<IdentityResult> SetLockoutEnabledAsync(string userId, bool lockoutIsEnabled)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            user.LockoutEnabled = lockoutIsEnabled;

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<DateTimeOffset> GetLockoutEndDateAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var lockoutEndDate = user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) : new DateTimeOffset();

            return lockoutEndDate;
        }

        public virtual async Task<IdentityResult> SetLockoutEndDateAsync(string userId, DateTimeOffset lockoutEndDate)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (!user.LockoutEnabled)
            {
                identityResult = new IdentityResult(false, new[] { Resource.LockoutNotEnabled });
            }
            else
            {
                user.LockoutEndDateUtc = lockoutEndDate == DateTimeOffset.MinValue ? new DateTime?() : lockoutEndDate.UtcDateTime;
                identityResult = await this.UpdateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> IncrementAccessFailedAttemptsAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            int accessFailedAttempts = user.AccessFailedCount++;
            if (accessFailedAttempts >= this.MaxFailedAccessAttemptsBeforeLockout)
            {
                user.LockoutEndDateUtc = DateTimeOffset.UtcNow.Add(this.DefaultAccountLockoutTimeSpan).UtcDateTime;
                user.AccessFailedCount = 0;
            }

            IdentityResult identityResult = await this.UpdateAsync(user);

            return identityResult;
        }

        public virtual async Task<IdentityResult> ResetAccessFailedAttemptsAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            IdentityResult identityResult;
            if (user.AccessFailedCount == 0)
            {
                identityResult = new IdentityResult(true, null);
            }
            else
            {
                user.AccessFailedCount = 0;
                identityResult = await this.UpdateAsync(user);
            }

            return identityResult;
        }

        public virtual async Task<int> GetAccessFailedAttemptsAsync(string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var accessFailedAttempts = user.AccessFailedCount;

            return accessFailedAttempts;
        }

        public virtual async Task<bool> ValidateUserTokenAsync(string userId, string modifier, string userToken)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var userTokenIsValid = await this.userTokenProvider.ValidateAsync(modifier, user, userToken);

            return userTokenIsValid;
        }

        public virtual async Task<string> GenerateUserTokenAsync(string purpose, string userId)
        {
            User user = await this.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.UserIdNotFound, userId));
            }

            var userToken = await this.userTokenProvider.GenerateAsync(purpose, user);

            return userToken;
        }

        private async Task<IdentityResult> UpdatePasswordAsync(User user, string password)
        {
            IdentityResult identityResult = await this.passwordIdentityValidator.ValidateAsync(password);
            if (identityResult.IsValid)
            {
                user.PasswordHash = this.passwordHasher.HashPassword(password);
                user.SecurityStamp = this.CreateSecurityStamp();
            }

            return identityResult;
        }

        private bool VerifyHashedPassword(User user, string password)
        {
            return this.passwordHasher.VerifyHashedPassword(user.PasswordHash, password);
        }

        private string CreateSecurityStamp()
        {
            var securityStamp = Guid.NewGuid().ToString();

            return securityStamp;
        }
    }
}