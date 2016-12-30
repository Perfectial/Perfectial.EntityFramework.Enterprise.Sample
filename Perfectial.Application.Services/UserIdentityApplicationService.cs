namespace Perfectial.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using AutoMapper;

    using Common.Logging;

    using Microsoft.Owin.Security;

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

        private const string IdentityClaimsNameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string IdentityClaimsEmailAddressIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        private const int DefaultMaxFailedAccessAttemptsBeforeLockout = 5;
        private const int DefaultAccountLockoutTimeSpanInMinutes = 5;

        private readonly IAuthenticationManager authenticationManager;
        private readonly IClaimsIdentityFactory claimsIdentityFactory;
        private readonly IPasswordHasher passwordHasher;
        private readonly IIdentityValidator<string> passwordIdentityValidator;
        private readonly IIdentityValidator<User> userIdentityValidator;
        private readonly IIdentityProvider userIdentityProvider;
        private readonly IUserRepository userRepository;
        private readonly IUserTokenProvider userTokenProvider;
        private readonly IUserTokenTwoFactorProvider userTokenTwoFactorProvider;

        public UserIdentityApplicationService(
            IAuthenticationManager authenticationManager,
            IClaimsIdentityFactory claimsIdentityFactory,
            IPasswordHasher passwordHasher,
            IIdentityValidator<string> passwordIdentityValidator,
            IIdentityValidator<User> userIdentityValidator,
            IUserRepository userRepository,
            IUserTokenProvider userTokenProvider,
            IIdentityProvider userIdentityProvider,
            IUserTokenTwoFactorProvider userTokenTwoFactorProvider,
            IDbContextScopeFactory dbContextScopeFactory,
            IMapper mapper,
            ILog logger)
            : base(dbContextScopeFactory, mapper, logger)
        {
            this.authenticationManager = authenticationManager;
            this.claimsIdentityFactory = claimsIdentityFactory;
            this.passwordHasher = passwordHasher;
            this.passwordIdentityValidator = passwordIdentityValidator;
            this.userIdentityValidator = userIdentityValidator;
            this.userRepository = userRepository;
            this.userTokenProvider = userTokenProvider;
            this.userIdentityProvider = userIdentityProvider;
            this.userTokenTwoFactorProvider = userTokenTwoFactorProvider;

            this.AuthenticationType = Infrastructure.Identity.Model.AuthenticationType.ApplicationCookie;
            this.UserLockoutEnabledByDefault = true;
            this.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(DefaultAccountLockoutTimeSpanInMinutes);
            this.MaxFailedAccessAttemptsBeforeLockout = DefaultMaxFailedAccessAttemptsBeforeLockout;
        }

        public string AuthenticationType { get; set; }
        public bool UserLockoutEnabledByDefault { get; set; }
        public int MaxFailedAccessAttemptsBeforeLockout { get; set; }
        public TimeSpan DefaultAccountLockoutTimeSpan { get; set; }

        public virtual async Task<IdentityResult> CreateAsync(User user)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
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
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> CreateAsync(User user, string password)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult = await this.UpdatePasswordAsync(user, password);
                if (identityResult.IsValid)
                {
                    identityResult = await this.CreateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> UpdateAsync(User user)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult = await this.userIdentityValidator.ValidateAsync(user);
                if (identityResult.IsValid)
                {
                    await this.userRepository.UpdateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> DeleteAsync(User user)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                await this.userRepository.DeleteAsync(user);
                await dbContextScope.SaveChangesAsync();
            }

            IdentityResult identityResult = new IdentityResult(true, null);

            return identityResult;
        }

        public virtual async Task<User> FindByIdAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var user = await this.userRepository.FindByIdAsync(userId);

                return user;
            }
        }

        public virtual async Task<User> FindByNameAsync(string userName)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var user = await this.userRepository.FindByNameAsync(userName);

                return user;
            }
        }

        public virtual async Task<User> FindByNameAsync(string userName, string password)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
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
        }

        public virtual async Task<bool> HasPasswordAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var userHasPassword = !string.IsNullOrEmpty(user.PasswordHash);

                return userHasPassword;
            }
        }

        public virtual async Task<IdentityResult> SetPasswordAsync(string userId, string password)
        {
            IdentityResult identityResult;

            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    identityResult = await this.UpdatePasswordAsync(user, password);
                    if (identityResult.IsValid)
                    {
                        identityResult = await this.UpdateAsync(user);
                        await dbContextScope.SaveChangesAsync();
                    }
                }
                else
                {
                    identityResult = new IdentityResult(false, new[] { Resource.UserAlreadyHasPassword });
                }
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            IdentityResult identityResult;

            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                if (this.VerifyHashedPassword(user, currentPassword))
                {
                    identityResult = await this.UpdatePasswordAsync(user, newPassword);
                    if (identityResult.IsValid)
                    {
                        identityResult = await this.UpdateAsync(user);
                        await dbContextScope.SaveChangesAsync();
                    }
                }
                else
                {
                    identityResult = new IdentityResult(false, new[] { Resource.PasswordMismatch });
                }
            }

            return identityResult;
        }

        public virtual async Task<IdentityResult> RemovePasswordAsync(string userId)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                user.PasswordHash = null;
                user.SecurityStamp = this.CreateSecurityStamp();

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> ResetPasswordAsync(string userId, string userToken, string newPassword)
        {
            IdentityResult identityResult;

            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                if (await this.ValidateUserTokenAsync(userId, ResetPasswordUserTokenModifier, userToken))
                {
                    identityResult = await this.UpdatePasswordAsync(user, newPassword);
                    if (identityResult.IsValid)
                    {
                        identityResult = await this.UpdateAsync(user);
                        await dbContextScope.SaveChangesAsync();
                    }
                }
                else
                {
                    identityResult = new IdentityResult(false, new[] { Resource.InvalidToken });
                }
            }

            return identityResult;
        }

        public virtual async Task<string> GetSecurityStampAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var securityStamp = user.SecurityStamp;

                return securityStamp;
            }
        }

        public virtual async Task<IdentityResult> UpdateSecurityStampAsync(string userId)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                user.SecurityStamp = this.CreateSecurityStamp();

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual Task<string> GenerateResetPasswordUserTokenAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var resetPasswordUserToken = this.GenerateUserTokenAsync(ResetPasswordUserTokenModifier, userId);

                return resetPasswordUserToken;
            }
        }

        public virtual async Task<IList<UserLinkedLogin>> GetLoginsAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var userLogins = await this.userRepository.GetLoginsAsync(user);

                return userLogins;
            }
        }

        public virtual async Task<IdentityResult> CreateLoginAsync(string userId, UserLinkedLogin userLinkedLogin)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User userFoundByLogin = await this.FindByLoginAsync(userLinkedLogin);
                if (userFoundByLogin != null)
                {
                    identityResult = new IdentityResult(false, new[] { Resource.ExternalLoginExists });
                }
                else
                {
                    User userFoundById = await this.FindByIdAsync(userId);

                    await this.userRepository.AddLoginAsync(userFoundById, userLinkedLogin);
                    identityResult = await this.UpdateAsync(userFoundById);
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> DeleteLoginAsync(string userId, UserLinkedLogin userLinkedLogin)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);

                await this.userRepository.RemoveLoginAsync(user, userLinkedLogin);
                user.SecurityStamp = this.CreateSecurityStamp();

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<User> FindByLoginAsync(UserLinkedLogin userLinkedLogin)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var user = await this.userRepository.FindByLoginAsync(userLinkedLogin);

                return user;
            }
        }

        public virtual async Task<IList<Claim>> GetClaimsAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var userClaims = await this.userRepository.GetClaimsAsync(user);

                return userClaims;
            }
        }

        public virtual async Task<IdentityResult> CreateClaimAsync(string userId, Claim claim)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                await this.userRepository.AddClaimAsync(user, claim);

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> DeleteClaimAsync(string userId, Claim claim)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                await this.userRepository.RemoveClaimAsync(user, claim);

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<IList<string>> GetRolesAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var userRoles = await this.userRepository.GetRolesAsync(user);

                return userRoles;
            }
        }

        public virtual async Task<IdentityResult> CreateRoleAsync(string userId, string role)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
                var userRoles = await this.userRepository.GetRolesAsync(user);
                if (userRoles.Contains(role))
                {
                    identityResult = new IdentityResult(false, new[] { Resource.UserAlreadyInRole });
                }
                else
                {
                    await this.userRepository.AddToRoleAsync(user, role);
                    identityResult = await this.UpdateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> CreateRolesAsync(string userId, params string[] roles)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
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
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> DeleteRoleAsync(string userId, string role)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
                if (!await this.userRepository.IsInRoleAsync(user, role))
                {
                    identityResult = new IdentityResult(false, new[] { Resource.UserNotInRole });
                }
                else
                {
                    await this.userRepository.RemoveFromRoleAsync(user, role);
                    identityResult = await this.UpdateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> DeleteRolesAsync(string userId, params string[] roles)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
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
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<bool> IsInRoleAsync(string userId, string role)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var isUserInRole = await this.userRepository.IsInRoleAsync(user, role);

                return isUserInRole;
            }
        }

        public virtual async Task<string> GetEmailAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var email = user.Email;

                return email;
            }
        }

        public virtual async Task<IdentityResult> SetEmailAsync(string userId, string email)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                user.Email = email;
                user.EmailConfirmed = false;
                user.SecurityStamp = this.CreateSecurityStamp();

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual Task<User> FindByEmailAsync(string email)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var user = this.userRepository.FindByEmailAsync(email);

                return user;
            }
        }

        public virtual Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var emailConfirmationToken = this.GenerateUserTokenAsync(EmailConfirmationUserTokenModifier, userId);

                return emailConfirmationToken;
            }
        }

        public virtual async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
                if (await this.ValidateUserTokenAsync(userId, EmailConfirmationUserTokenModifier, token))
                {
                    user.EmailConfirmed = true;
                    identityResult = await this.UpdateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }
                else
                {
                    identityResult = new IdentityResult(false, new[] { Resource.InvalidToken });
                }

                return identityResult;
            }
        }

        public virtual async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var emailIsConfirmed = user.EmailConfirmed;

                return emailIsConfirmed;
            }
        }

        public virtual async Task<string> GetPhoneNumberAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var phoneNumber = user.PhoneNumber;

                return phoneNumber;
            }
        }

        public virtual async Task<IdentityResult> SetPhoneNumberAsync(string userId, string phoneNumber)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                user.PhoneNumber = phoneNumber;
                user.EmailConfirmed = false;
                user.SecurityStamp = this.CreateSecurityStamp();

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> UpdatePhoneNumberAsync(string userId, string phoneNumber, string token)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
                if (await this.ValidateUserTokenAsync(userId, phoneNumber, token))
                {
                    user.PhoneNumber = phoneNumber;
                    user.PhoneNumberConfirmed = true;
                    user.SecurityStamp = this.CreateSecurityStamp();

                    identityResult = await this.UpdateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }
                else
                {
                    identityResult = new IdentityResult(false, new[] { Resource.InvalidToken });
                }

                return identityResult;
            }
        }

        public virtual async Task<bool> IsPhoneNumberConfirmedAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var phoneNumberConfirmed = user.PhoneNumberConfirmed;

                return phoneNumberConfirmed;
            }
        }

        public virtual Task<string> GeneratePhoneNumberConfirmationTokenAsync(string userId, string phoneNumber)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var phoneNumberConfirmationToken = this.GenerateUserTokenAsync(PhoneNumberConfirmationUserTokenModifier, userId);

                return phoneNumberConfirmationToken;
            }
        }

        public virtual void RegisterTwoFactorAuthenticationProvider(string twoFactorProviderKey, IUserTokenProvider provider)
        {
            this.userTokenTwoFactorProvider.RegisterTwoFactorUserTokenProvider(twoFactorProviderKey, provider);
        }

        public virtual async Task<IList<string>> GetValidTwoFactorAuthenticationProvidersAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var validTwoFactorUserTokenProviders = await this.userTokenTwoFactorProvider.GetValidTwoFactorUserTokenProvidersAsync(user);

                return validTwoFactorUserTokenProviders;
            }
        }

        public virtual async Task<string> GenerateTwoFactorAuthenticationUserTokenAsync(string userId, string twoFactorProviderKey)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var twoFactorUserToken = await this.userTokenTwoFactorProvider.GenerateTwoFactorUserTokenAsync(twoFactorProviderKey, user);

                return twoFactorUserToken;
            }
        }

        public virtual async Task<bool> ValidateTwoFactorAuthenticationProviderAsync(string userId, string twoFactorProviderKey, string token)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var twoFactorUserTokenIsValid = await this.userTokenTwoFactorProvider.ValidateTwoFactorUserTokenAsync(twoFactorProviderKey, user, token);

                return twoFactorUserTokenIsValid;
            }
        }

        public virtual async Task NotifyTwoFactorAuthenticationProviderAsync(string userId, string twoFactorProviderKey, string token)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);

                await this.userTokenTwoFactorProvider.NotifyTwoFactorUserTokenAsync(twoFactorProviderKey, user, token);
            }
        }

        public virtual async Task<bool> GetTwoFactorUserTokenAuthenticationIsEnabledAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var twoFactorUserTokenAuthenticationIsEnabled = user.TwoFactorEnabled;

                return twoFactorUserTokenAuthenticationIsEnabled;
            }
        }

        public virtual async Task<IdentityResult> SetTwoFactorUserTokenAuthenticationEnabledAsync(string userId, bool twoFactorUserTokenAuthenticationIsEnabled)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                user.TwoFactorEnabled = twoFactorUserTokenAuthenticationIsEnabled;
                user.SecurityStamp = this.CreateSecurityStamp();

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<bool> IsLockedOutAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);

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
        }

        public virtual async Task<bool> GetLockoutEnabledAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var lockoutIsEnabled = user.LockoutEnabled;

                return lockoutIsEnabled;
            }
        }

        public virtual async Task<IdentityResult> SetLockoutEnabledAsync(string userId, bool lockoutIsEnabled)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                user.LockoutEnabled = lockoutIsEnabled;

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<DateTimeOffset> GetLockoutEndDateAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var lockoutEndDate = user.LockoutEndDateUtc.HasValue ?
                    new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) :
                    new DateTimeOffset();

                return lockoutEndDate;
            }
        }

        public virtual async Task<IdentityResult> SetLockoutEndDateAsync(string userId, DateTimeOffset lockoutEndDate)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
                if (!user.LockoutEnabled)
                {
                    identityResult = new IdentityResult(false, new[] { Resource.LockoutNotEnabled });
                }
                else
                {
                    user.LockoutEndDateUtc = lockoutEndDate == DateTimeOffset.MinValue ? new DateTime?() : lockoutEndDate.UtcDateTime;
                    identityResult = await this.UpdateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> IncrementAccessFailedAttemptsAsync(string userId)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.FindByIdAsync(userId);
                int accessFailedAttempts = user.AccessFailedCount++;
                if (accessFailedAttempts >= this.MaxFailedAccessAttemptsBeforeLockout)
                {
                    user.LockoutEndDateUtc = DateTimeOffset.UtcNow.Add(this.DefaultAccountLockoutTimeSpan).UtcDateTime;
                    user.AccessFailedCount = 0;
                }

                IdentityResult identityResult = await this.UpdateAsync(user);
                await dbContextScope.SaveChangesAsync();

                return identityResult;
            }
        }

        public virtual async Task<IdentityResult> ResetAccessFailedAttemptsAsync(string userId)
        {
            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                IdentityResult identityResult;

                User user = await this.FindByIdAsync(userId);
                if (user.AccessFailedCount == 0)
                {
                    identityResult = new IdentityResult(true, null);
                }
                else
                {
                    user.AccessFailedCount = 0;
                    identityResult = await this.UpdateAsync(user);
                    await dbContextScope.SaveChangesAsync();
                }

                return identityResult;
            }
        }

        public virtual async Task<int> GetAccessFailedAttemptsAsync(string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var accessFailedAttempts = user.AccessFailedCount;

                return accessFailedAttempts;
            }
        }

        public virtual async Task<bool> ValidateUserTokenAsync(string userId, string modifier, string userToken)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var userTokenIsValid = await this.userTokenProvider.ValidateAsync(modifier, user, userToken);

                return userTokenIsValid;
            }
        }

        public virtual async Task<string> GenerateUserTokenAsync(string purpose, string userId)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.FindByIdAsync(userId);
                var userToken = await this.userTokenProvider.GenerateAsync(purpose, user);

                return userToken;
            }
        }

        public async Task<SignInStatus> SignInAsync(User user, bool isPersistentCookie, bool rememberBrowserCookie)
        {
            SignInStatus signInStatus;

            ClaimsIdentity userClaimsIdentity = await this.CreateUserClaimsIdentityAsync(user);
            this.authenticationManager.SignOut(Infrastructure.Identity.Model.AuthenticationType.ExternalCookie, Infrastructure.Identity.Model.AuthenticationType.TwoFactorCookie);

            if (rememberBrowserCookie)
            {
                ClaimsIdentity rememberBrowserUserClaimsIdentity = this.CreateUserClaimsIdentity(user.Id, Infrastructure.Identity.Model.AuthenticationType.TwoFactorRememberBrowserCookie);
                this.authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistentCookie }, userClaimsIdentity, rememberBrowserUserClaimsIdentity);

                signInStatus = SignInStatus.Success;
            }
            else
            {
                this.authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistentCookie }, userClaimsIdentity);

                signInStatus = SignInStatus.Success;
            }

            return signInStatus;
        }

        public async Task<string> GetTwoFactorAuthenticationUserIdAsync()
        {
            string verifiedUserId = string.Empty;

            AuthenticateResult authenticateResult = await this.authenticationManager.AuthenticateAsync(Infrastructure.Identity.Model.AuthenticationType.TwoFactorCookie);
            if (authenticateResult?.Identity != null)
            {
                verifiedUserId = this.userIdentityProvider.GetUserId(authenticateResult.Identity);
            }

            return verifiedUserId;
        }

        public async Task<bool> SendTwoFactorAuthenticationUserTokenAsync(string twoFactorProviderKey)
        {
            bool twoFactorCodeIsSent = false;

            string userId = await this.GetTwoFactorAuthenticationUserIdAsync();
            if (userId != null)
            {
                using (this.DbContextScopeFactory.CreateReadOnly())
                {
                    User user = await this.userRepository.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var twoFactorUserToken = await this.userTokenTwoFactorProvider.GenerateTwoFactorUserTokenAsync(twoFactorProviderKey, user);
                        await this.userTokenTwoFactorProvider.NotifyTwoFactorUserTokenAsync(twoFactorProviderKey, user, twoFactorUserToken);

                        twoFactorCodeIsSent = true;
                    }
                }
            }

            return twoFactorCodeIsSent;
        }

        public async Task<SignInStatus> SignInWithTwoFactorAuthenticationAsync(string twoFactorProviderKey, string token, bool isPersistentCookie, bool rememberBrowserCookie)
        {
            SignInStatus signInStatus;

            var userId = await this.GetTwoFactorAuthenticationUserIdAsync();
            if (userId == null)
            {
                signInStatus = SignInStatus.Failure;
            }
            else
            {
                using (var dbContextScope = this.DbContextScopeFactory.Create())
                {
                    User user = await this.userRepository.FindByIdAsync(userId);
                    if (user == null)
                    {
                        signInStatus = SignInStatus.Failure;
                    }
                    else
                    {
                        bool userIsLockedOut = user.LockoutEnabled && user.LockoutEndDateUtc >= DateTimeOffset.UtcNow;
                        if (userIsLockedOut)
                        {
                            signInStatus = SignInStatus.LockedOut;
                        }
                        else
                        {
                            var twoFactorUserTokenIsValid = await this.userTokenTwoFactorProvider.ValidateTwoFactorUserTokenAsync(twoFactorProviderKey, user, token);
                            if (twoFactorUserTokenIsValid)
                            {
                                if (user.AccessFailedCount != 0)
                                {
                                    user.AccessFailedCount = 0;
                                    await this.UpdateAsync(user);
                                    await dbContextScope.SaveChangesAsync();
                                }

                                signInStatus = await this.SignInAsync(user, isPersistentCookie, rememberBrowserCookie);
                            }
                            else
                            {
                                int accessFailedAttempts = user.AccessFailedCount++;
                                if (accessFailedAttempts >= this.MaxFailedAccessAttemptsBeforeLockout)
                                {
                                    user.LockoutEndDateUtc = DateTimeOffset.UtcNow.Add(this.DefaultAccountLockoutTimeSpan).UtcDateTime;
                                    user.AccessFailedCount = 0;
                                }

                                await this.UpdateAsync(user);
                                await dbContextScope.SaveChangesAsync();

                                signInStatus = SignInStatus.Failure;
                            }
                        }
                    }
                }
            }

            return signInStatus;
        }

        public void SignOut()
        {
            this.authenticationManager.SignOut(Infrastructure.Identity.Model.AuthenticationType.ApplicationCookie);
        }

        public async Task<UserExternalLogin> GetUserExternalLoginAsync()
        {
            var authenticateResult = await this.authenticationManager.AuthenticateAsync(Infrastructure.Identity.Model.AuthenticationType.ExternalCookie);

            return this.GetUserExternalLogin(authenticateResult);
        }

        public async Task<UserExternalLogin> GetUserExternalLoginAsync(string xsrfKey, string expectedValue)
        {
            var authenticateResult = await this.authenticationManager.AuthenticateAsync(Infrastructure.Identity.Model.AuthenticationType.ExternalCookie);

            return authenticateResult?.Properties?.Dictionary == null
                || !authenticateResult.Properties.Dictionary.ContainsKey(xsrfKey)
                || authenticateResult.Properties.Dictionary[xsrfKey] != expectedValue ? null : this.GetUserExternalLogin(authenticateResult);
        }

        public async Task<SignInStatus> SignInWithUserExternalLoginAsync(UserExternalLogin userExternalLogin, bool isPersistentCookie)
        {
            SignInStatus signInStatus;

            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                User user = await this.userRepository.FindByLoginAsync(userExternalLogin.LinkedLogin);
                if (user == null)
                {
                    signInStatus = SignInStatus.Failure;
                }
                else
                {
                    bool userIsLockedOut = user.LockoutEnabled && user.LockoutEndDateUtc >= DateTimeOffset.UtcNow;
                    if (userIsLockedOut)
                    {
                        signInStatus = SignInStatus.LockedOut;
                    }
                    else
                    {
                        if (user.TwoFactorEnabled)
                        {
                            signInStatus = await this.SignInWithTwoFactorCookieAsync(user);
                        }
                        else
                        {
                            signInStatus = await this.SignInAsync(user, isPersistentCookie, false);
                        }
                    }
                }
            }

            return signInStatus;
        }

        public async Task<SignInStatus> SignInWithTwoFactorCookieAsync(User user)
        {
            SignInStatus signInStatus = SignInStatus.Failure;

            var validTwoFactorUserTokenProviders = await this.userTokenTwoFactorProvider.GetValidTwoFactorUserTokenProvidersAsync(user);
            if (validTwoFactorUserTokenProviders.Count > 0)
            {
                if (!await this.AuthenticateIdentityAsync(user.Id, Infrastructure.Identity.Model.AuthenticationType.TwoFactorRememberBrowserCookie))
                {
                    ClaimsIdentity claimsIdentity = this.CreateUserClaimsIdentity(user.Id, Infrastructure.Identity.Model.AuthenticationType.TwoFactorCookie);
                    this.authenticationManager.SignIn(claimsIdentity);

                    signInStatus = SignInStatus.RequiresVerification;
                }
            }

            return signInStatus;
        }

        public async Task<SignInStatus> SignInWithUsernameAndPasswordAsync(string userName, string password, bool isPersistentCookie, bool shouldLockout)
        {
            SignInStatus signInStatus;

            using (var dbContextScope = this.DbContextScopeFactory.Create())
            {
                User user = await this.userRepository.FindByNameAsync(userName);
                if (user == null)
                {
                    signInStatus = SignInStatus.Failure;
                }
                else
                {
                    bool userIsLockedOut = user.LockoutEnabled && user.LockoutEndDateUtc >= DateTimeOffset.UtcNow;
                    if (userIsLockedOut)
                    {
                        signInStatus = SignInStatus.LockedOut;
                    }
                    else
                    {
                        if (this.passwordHasher.VerifyHashedPassword(user.PasswordHash, password))
                        {
                            if (user.AccessFailedCount != 0)
                            {
                                user.AccessFailedCount = 0;
                                await this.UpdateAsync(user);
                                await dbContextScope.SaveChangesAsync();
                            }

                            if (user.TwoFactorEnabled)
                            {
                                signInStatus = await this.SignInWithTwoFactorCookieAsync(user);
                            }
                            else
                            {
                                signInStatus = await this.SignInAsync(user, isPersistentCookie, false);
                            }
                        }
                        else
                        {
                            if (shouldLockout)
                            {
                                int accessFailedAttempts = user.AccessFailedCount++;
                                if (accessFailedAttempts >= this.MaxFailedAccessAttemptsBeforeLockout)
                                {
                                    user.LockoutEndDateUtc = DateTimeOffset.UtcNow.Add(this.DefaultAccountLockoutTimeSpan).UtcDateTime;
                                    user.AccessFailedCount = 0;
                                }

                                await this.UpdateAsync(user);
                                await dbContextScope.SaveChangesAsync();

                                userIsLockedOut = user.LockoutEnabled && user.LockoutEndDateUtc >= DateTimeOffset.UtcNow;
                                signInStatus = userIsLockedOut ? SignInStatus.LockedOut : SignInStatus.Failure;
                            }
                            else
                            {
                                signInStatus = SignInStatus.Failure;
                            }
                        }
                    }
                }
            }

            return signInStatus;
        }

        public virtual async Task<ClaimsIdentity> CreateUserClaimsIdentityAsync(User user)
        {
            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var userRoles = await this.userRepository.GetRolesAsync(user);
                var userClaims = await this.userRepository.GetClaimsAsync(user);
                var userClaimsIdentity = await this.claimsIdentityFactory.CreateAsync(user, this.AuthenticationType, userRoles, userClaims);

                return userClaimsIdentity;
            }
        }

        public async Task<bool> AuthenticateIdentityAsync(string userId, string authenticationType)
        {
            var authenticateResult = await this.authenticationManager.AuthenticateAsync(authenticationType);

            return authenticateResult?.Identity != null && this.userIdentityProvider.GetUserId(authenticateResult.Identity) == userId;
        }

        public IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes()
        {
            return this.authenticationManager.GetAuthenticationTypes(authenticationDescription =>
            {
                if (authenticationDescription.Properties != null)
                {
                    return authenticationDescription.Properties.ContainsKey("Caption");
                }

                return false;
            });
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

        private ClaimsIdentity CreateUserClaimsIdentity(string userId, string authenticationType)
        {
            var claimsIdentity = new ClaimsIdentity(authenticationType);
            claimsIdentity.AddClaim(new Claim(IdentityClaimsNameIdentifier, userId));

            return claimsIdentity;
        }

        private UserExternalLogin GetUserExternalLogin(AuthenticateResult result)
        {
            UserExternalLogin userExternalLogin = null;

            Claim firstClaim = result?.Identity?.FindFirst(IdentityClaimsNameIdentifier);
            if (firstClaim != null)
            {
                string identityName = result.Identity.Name;
                identityName = identityName?.Replace(" ", string.Empty);

                string emailAddress = result.Identity.FindFirst(IdentityClaimsEmailAddressIdentifier)?.Value;
                userExternalLogin = new UserExternalLogin
                {
                    ExternalIdentity = result.Identity,
                    LinkedLogin = new UserLinkedLogin(firstClaim.Issuer, firstClaim.Value),
                    DefaultUserName = identityName,
                    Email = emailAddress
                };
            }

            return userExternalLogin;
        }
    }
}