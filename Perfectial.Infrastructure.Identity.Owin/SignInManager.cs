namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Model;

    using TaskExtensions = Perfectial.Infrastructure.Identity.Base.TaskExtensions;

    /// <summary>
    /// Manages Sign In operations for users
    /// </summary>
    /// <typeparam name="TUser"/><typeparam name="TPrimaryKey"/>
    public class SignInManager<TUser, TPrimaryKey> : IDisposable where TUser : IEntity<TPrimaryKey>, Domain.Model.IUser
    {
        private string _authType;

        /// <summary>
        /// AuthenticationType that will be used by sign in, defaults to DefaultAuthenticationTypes.ApplicationCookie
        /// 
        /// </summary>
        public string AuthenticationType
        {
            get
            {
                return this._authType ?? "ApplicationCookie";
            }
            set
            {
                this._authType = value;
            }
        }

        /// <summary>
        /// Used to operate on users
        /// 
        /// </summary>
        public Infrastructure.Identity.UserManager<TUser, TPrimaryKey> UserManager { get; set; }

        /// <summary>
        /// Used to sign in identities
        /// 
        /// </summary>
        public IAuthenticationManager AuthenticationManager { get; set; }

        public SignInManager(Infrastructure.Identity.UserManager<TUser, TPrimaryKey> userManager, IAuthenticationManager authenticationManager)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (authenticationManager == null)
                throw new ArgumentNullException(nameof(authenticationManager));
            this.UserManager = userManager;
            this.AuthenticationManager = authenticationManager;
        }

        /// <summary>
        /// Called to generate the ClaimsIdentity for the user, override to add additional claims before SignIn
        /// 
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public virtual Task<ClaimsIdentity> CreateUserIdentityAsync(TUser user)
        {
            return this.UserManager.CreateIdentityAsync(user, this.AuthenticationType);
        }

        /// <summary>
        /// Convert a TKey userId to a string, by default this just calls ToString()
        /// 
        /// </summary>
        /// <param name="id"/>
        /// <returns/>
        public virtual string ConvertIdToString(TPrimaryKey id)
        {
            return Convert.ToString((object)id, (IFormatProvider)CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert a string id to the proper TKey using Convert.ChangeType
        /// 
        /// </summary>
        /// <param name="id"/>
        /// <returns/>
        public virtual TPrimaryKey ConvertIdFromString(string id)
        {
            if (id == null)
                return default(TPrimaryKey);
            return (TPrimaryKey)Convert.ChangeType((object)id, typeof(TPrimaryKey), (IFormatProvider)CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates a user identity and then signs the identity using the AuthenticationManager
        /// 
        /// </summary>
        /// <param name="user"/><param name="isPersistent"/><param name="rememberBrowser"/>
        /// <returns/>
        public virtual async Task SignInAsync(TUser user, bool isPersistent, bool rememberBrowser)
        {
            ClaimsIdentity userIdentity = await TaskExtensions.WithCurrentCulture(this.CreateUserIdentityAsync(user));
            this.AuthenticationManager.SignOut(new string[2]
            {
        "ExternalCookie",
        "TwoFactorCookie"
            });
            if (rememberBrowser)
            {
                ClaimsIdentity rememberBrowserIdentity = this.AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(this.ConvertIdToString(user.Id));
                IAuthenticationManager authenticationManager = this.AuthenticationManager;
                AuthenticationProperties authenticationProperties1 = new AuthenticationProperties();
                //authenticationProperties1.set_IsPersistent(isPersistent);
                authenticationProperties1.IsPersistent = isPersistent;
                AuthenticationProperties authenticationProperties2 = authenticationProperties1;
                ClaimsIdentity[] claimsIdentityArray = new ClaimsIdentity[2]
                {
          userIdentity,
          rememberBrowserIdentity
                };
                authenticationManager.SignIn(authenticationProperties2, claimsIdentityArray);
            }
            else
            {
                IAuthenticationManager authenticationManager = this.AuthenticationManager;
                AuthenticationProperties authenticationProperties1 = new AuthenticationProperties();
                // authenticationProperties1.set_IsPersistent(isPersistent);
                authenticationProperties1.IsPersistent = isPersistent;
                AuthenticationProperties authenticationProperties2 = authenticationProperties1;
                ClaimsIdentity[] claimsIdentityArray = new ClaimsIdentity[1]
                {
          userIdentity
                };
                authenticationManager.SignIn(authenticationProperties2, claimsIdentityArray);
            }
        }

        /// <summary>
        /// Send a two factor code to a user
        /// 
        /// </summary>
        /// <param name="provider"/>
        /// <returns/>
        public virtual async Task<bool> SendTwoFactorCodeAsync(string provider)
        {
            TPrimaryKey userId = (TPrimaryKey)await TaskExtensions.WithCurrentCulture<TPrimaryKey>(this.GetVerifiedUserIdAsync());
            bool flag;
            if ((object)userId == null)
            {
                flag = false;
            }
            else
            {
                string token = (string)await TaskExtensions.WithCurrentCulture<string>(this.UserManager.GenerateTwoFactorTokenAsync(userId, provider));
                IdentityResult identityResult = await TaskExtensions.WithCurrentCulture<IdentityResult>(this.UserManager.NotifyTwoFactorTokenAsync(userId, provider, token));
                flag = true;
            }
            return flag;
        }

        /// <summary>
        /// Get the user id that has been verified already or null.
        /// 
        /// </summary>
        /// 
        /// <returns/>
        public async Task<TPrimaryKey> GetVerifiedUserIdAsync()
        {
            AuthenticateResult result = await TaskExtensions.WithCurrentCulture(this.AuthenticationManager.AuthenticateAsync("TwoFactorCookie"));
            return result?.Identity == null || string.IsNullOrEmpty(result.Identity.GetUserId()) ? default(TPrimaryKey) : this.ConvertIdFromString(result.Identity.GetUserId());
        }

        /// <summary>
        /// Has the user been verified (ie either via password or external login)
        /// 
        /// </summary>
        /// 
        /// <returns/>
        public async Task<bool> HasBeenVerifiedAsync()
        {
            return await TaskExtensions.WithCurrentCulture(this.GetVerifiedUserIdAsync()) != null;
        }

        /// <summary>
        /// Two factor verification step
        /// 
        /// </summary>
        /// <param name="provider"/><param name="code"/><param name="isPersistent"/><param name="rememberBrowser"/>
        /// <returns/>
        public virtual async Task<SignInStatus> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser)
        {
            TPrimaryKey userId = await TaskExtensions.WithCurrentCulture(this.GetVerifiedUserIdAsync());
            SignInStatus signInStatus;
            if (userId == null)
            {
                signInStatus = SignInStatus.Failure;
            }
            else
            {
                TUser user = await TaskExtensions.WithCurrentCulture(this.UserManager.FindByIdAsync(userId));
                if ((object)user == null)
                    signInStatus = SignInStatus.Failure;
                else if (await TaskExtensions.WithCurrentCulture<bool>(this.UserManager.IsLockedOutAsync(user.Id)))
                    signInStatus = SignInStatus.LockedOut;
                else if (await TaskExtensions.WithCurrentCulture<bool>(this.UserManager.VerifyTwoFactorTokenAsync(user.Id, provider, code)))
                {
                    IdentityResult identityResult = await TaskExtensions.WithCurrentCulture(this.UserManager.ResetAccessFailedCountAsync(user.Id));
                    await TaskExtensions.WithCurrentCulture(this.SignInAsync(user, isPersistent, rememberBrowser));
                    signInStatus = SignInStatus.Success;
                }
                else
                {
                    IdentityResult identityResult = await TaskExtensions.WithCurrentCulture(this.UserManager.AccessFailedAsync(user.Id));
                    signInStatus = SignInStatus.Failure;
                }
            }
            return signInStatus;
        }

        /// <summary>
        /// Sign the user in using an associated external login
        /// 
        /// </summary>
        /// <param name="externalLogin"/><param name="isPersistent"/>
        /// <returns/>
        public async Task<SignInStatus> ExternalSignInAsync(ExternalLogin externalLogin, bool isPersistent)
        {
            TUser user = await TaskExtensions.WithCurrentCulture(this.UserManager.FindAsync(externalLogin.Login));
            SignInStatus signInStatus;
            if (user == null)
                signInStatus = SignInStatus.Failure;
            else if (await TaskExtensions.WithCurrentCulture(this.UserManager.IsLockedOutAsync(user.Id)))
                signInStatus = SignInStatus.LockedOut;
            else
                signInStatus = await TaskExtensions.WithCurrentCulture(this.SignInOrTwoFactor(user, isPersistent));
            return signInStatus;
        }

        private async Task<SignInStatus> SignInOrTwoFactor(TUser user, bool isPersistent)
        {
            string id = Convert.ToString((object)user.Id);
            SignInStatus signInStatus;
            if (await TaskExtensions.WithCurrentCulture<bool>(this.UserManager.GetTwoFactorEnabledAsync(user.Id)))
            {
                if ((await TaskExtensions.WithCurrentCulture(this.UserManager.GetValidTwoFactorProvidersAsync(user.Id))).Count > 0)
                {
                    if (!await TaskExtensions.WithCurrentCulture<bool>(this.AuthenticationManager.TwoFactorBrowserRememberedAsync(id)))
                    {
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity("TwoFactorCookie");
                        claimsIdentity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", id));
                        this.AuthenticationManager.SignIn(new ClaimsIdentity[1]
                        {
              claimsIdentity
                        });
                        signInStatus = SignInStatus.RequiresVerification;
                        goto label_9;
                    }
                }
            }
            await TaskExtensions.WithCurrentCulture(this.SignInAsync(user, isPersistent, false));
            signInStatus = SignInStatus.Success;
            label_9:
            return signInStatus;
        }

        /// <summary>
        /// Sign in the user in using the user name and password
        /// 
        /// </summary>
        /// <param name="userName"/><param name="password"/><param name="isPersistent"/><param name="shouldLockout"/>
        /// <returns/>
        public virtual async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            SignInStatus signInStatus;
            if (this.UserManager == null)
            {
                signInStatus = SignInStatus.Failure;
            }
            else
            {
                TUser user = await TaskExtensions.WithCurrentCulture(this.UserManager.FindByNameAsync(userName));
                if ((object)user == null)
                    signInStatus = SignInStatus.Failure;
                else if (await TaskExtensions.WithCurrentCulture<bool>(this.UserManager.IsLockedOutAsync(user.Id)))
                    signInStatus = SignInStatus.LockedOut;
                else if (await TaskExtensions.WithCurrentCulture<bool>(this.UserManager.CheckPasswordAsync(user, password)))
                {
                    IdentityResult identityResult = await TaskExtensions.WithCurrentCulture<IdentityResult>(this.UserManager.ResetAccessFailedCountAsync(user.Id));
                    signInStatus = (SignInStatus)await TaskExtensions.WithCurrentCulture<SignInStatus>(this.SignInOrTwoFactor(user, isPersistent));
                }
                else
                {
                    if (shouldLockout)
                    {
                        IdentityResult identityResult = await TaskExtensions.WithCurrentCulture<IdentityResult>(this.UserManager.AccessFailedAsync(user.Id));
                        if (await TaskExtensions.WithCurrentCulture<bool>(this.UserManager.IsLockedOutAsync(user.Id)))
                        {
                            signInStatus = SignInStatus.LockedOut;
                            goto label_19;
                        }
                    }
                    signInStatus = SignInStatus.Failure;
                }
            }
            label_19:
            return signInStatus;
        }

        /// <summary>
        /// Dispose
        /// 
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        /// <summary>
        /// If disposing, calls dispose on the Context.  Always nulls out the Context
        /// </summary>
        /// <param name="disposing"/>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}