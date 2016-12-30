namespace Perfectial.Presentation.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Perfectial.Application.Services.Base;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Presentation.Web.Models;
    using Perfectial.Presentation.Web.Resources;
    using Perfectial.Presentation.Web.ViewModels;

    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserIdentityApplicationService userIdentityApplicationService;
        private readonly IIdentityProvider userIdentityProvider;

        public AccountController(IUserIdentityApplicationService userIdentityApplicationService, IIdentityProvider identityProvider)
        {
            this.userIdentityApplicationService = userIdentityApplicationService;
            this.userIdentityProvider = identityProvider;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            var loginViewModel = new LoginViewModel(this.userIdentityProvider)
            {
                ReturnUrl = returnUrl,
                ExternalAuthenticationTypes = this.userIdentityApplicationService.GetExternalAuthenticationTypes().ToList()
            };

            return this.View(loginViewModel);
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                var signInStatus = await this.userIdentityApplicationService.SignInWithUsernameAndPasswordAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberBrowserCookie, shouldLockout: false);
                switch (signInStatus)
                {
                    case SignInStatus.Success:
                        return this.RedirectToLocalUrl(returnUrl);

                    case SignInStatus.LockedOut:
                        var lockoutViewModel = new LockoutViewModel(this.userIdentityProvider);

                        return this.View("Lockout", lockoutViewModel);

                    case SignInStatus.RequiresVerification:
                        return this.RedirectToAction(nameof(this.SendCode), "Account", new { ReturnUrl = returnUrl, loginViewModel.RememberBrowserCookie });

                    case SignInStatus.Failure:
                    default:
                        this.ModelState.AddModelError(string.Empty, Resource.AccountControllerLoginErrorMessage);
                        return this.View(loginViewModel);
                }
            }

            return this.View(loginViewModel);
        }

        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string providerKey, string returnUrl, bool rememberBrowserCookie)
        {
            var userId = await this.userIdentityApplicationService.GetTwoFactorAuthenticationUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                var errorViewModel = new ErrorViewModel(this.userIdentityProvider);

                return this.View("Error", errorViewModel);
            }

            var verifyCodeViewModel = new VerifyCodeViewModel(this.userIdentityProvider)
            {
                ProviderKey = providerKey,
                ReturnUrl = returnUrl,
                RememberBrowserCookie = rememberBrowserCookie
            };

            return this.View(verifyCodeViewModel);
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel verifyCodeViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var signInStatus = await this.userIdentityApplicationService.SignInWithTwoFactorAuthenticationAsync(verifyCodeViewModel.ProviderKey, verifyCodeViewModel.Code, verifyCodeViewModel.RememberBrowserCookie, verifyCodeViewModel.RememberBrowser);
                switch (signInStatus)
                {
                    case SignInStatus.Success:
                        return this.RedirectToLocalUrl(verifyCodeViewModel.ReturnUrl);

                    case SignInStatus.LockedOut:
                        var lockoutViewModel = new LockoutViewModel(this.userIdentityProvider);

                        return this.View("Lockout", lockoutViewModel);

                    case SignInStatus.Failure:
                    default:
                        this.ModelState.AddModelError(string.Empty, Resource.AccountControllerVerifyCodeErrorMessage);
                        return this.View(verifyCodeViewModel);
                }
            }

            return this.View(verifyCodeViewModel);
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var registerViewModel = new RegisterViewModel(this.userIdentityProvider);

            return this.View(registerViewModel);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var user = new User { UserName = registerViewModel.Email, Email = registerViewModel.Email };
                var identityResult = await this.userIdentityApplicationService.CreateAsync(user, registerViewModel.Password);
                if (identityResult.IsValid)
                {
                    await this.userIdentityApplicationService.SignInAsync(user, false, false);
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    return this.RedirectToAction("Index", "Home");
                }

                this.AddModelErrors(identityResult);
            }

            return this.View(registerViewModel);
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId != null && code != null)
            {
                var identityResult = await this.userIdentityApplicationService.ConfirmEmailAsync(userId, code);
                if (identityResult.IsValid)
                {
                    var confirmEmailViewModel = new ConfirmEmailViewModel(this.userIdentityProvider);

                    return this.View(confirmEmailViewModel);
                }
            }

            var errorViewModel = new ErrorViewModel(this.userIdentityProvider);

            return this.View("Error", errorViewModel);
        }

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            var forgotPasswordViewModel = new ForgotPasswordViewModel(this.userIdentityProvider);

            return this.View(forgotPasswordViewModel);
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.userIdentityApplicationService.FindByNameAsync(forgotPasswordViewModel.Email);
                if (user == null || !(await this.userIdentityApplicationService.IsEmailConfirmedAsync(user.Id)))
                {
                    var forgotPasswordConfirmationViewModel = new ForgotPasswordConfirmationViewModel(this.userIdentityProvider);

                    // Don't reveal that the user does not exist or is not confirmed.
                    return this.View(nameof(this.ForgotPasswordConfirmation), forgotPasswordConfirmationViewModel);
                }

                // TODO: Enable account confirmation.

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            return this.View(forgotPasswordViewModel);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            var forgotPasswordConfirmationViewModel = new ForgotPasswordConfirmationViewModel(this.userIdentityProvider);

            return this.View(forgotPasswordConfirmationViewModel);
        }

        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            if (code == null)
            {
                var errorViewModel = new ErrorViewModel(this.userIdentityProvider);

                return this.View("Error", errorViewModel);
            }

            var resetPasswordViewModel = new ResetPasswordViewModel(this.userIdentityProvider);

            return this.View(resetPasswordViewModel);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.userIdentityApplicationService.FindByNameAsync(resetPasswordViewModel.Email);
                if (user == null)
                {
                    var resetPasswordConfirmationViewModel = new ResetPasswordConfirmationViewModel(this.userIdentityProvider);

                    // Don't reveal that the user does not exist.
                    return this.RedirectToAction(nameof(this.ResetPasswordConfirmation), "Account", resetPasswordConfirmationViewModel);
                }

                var identityResult = await this.userIdentityApplicationService.ResetPasswordAsync(user.Id, resetPasswordViewModel.Token, resetPasswordViewModel.Password);
                if (identityResult.IsValid)
                {
                    var resetPasswordConfirmationViewModel = new ResetPasswordConfirmationViewModel(this.userIdentityProvider);

                    return this.RedirectToAction(nameof(this.ResetPasswordConfirmation), "Account", resetPasswordConfirmationViewModel);
                }

                this.AddModelErrors(identityResult);
            }

            return this.View(resetPasswordViewModel);
        }

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            var resetPasswordConfirmationViewModel = new ResetPasswordConfirmationViewModel(this.userIdentityProvider);

            return this.View(resetPasswordConfirmationViewModel);
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string providerKey, string returnUrl)
        {
            var challengeResult = new ChallengeResult(providerKey, this.Url.Action(nameof(this.ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl }));

            return challengeResult;
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var userExternalLogin = await this.userIdentityApplicationService.GetUserExternalLoginAsync();
            if (userExternalLogin != null)
            {
                var signInStatus = await this.userIdentityApplicationService.SignInWithUserExternalLoginAsync(userExternalLogin, false);
                switch (signInStatus)
                {
                    case SignInStatus.Success:
                        return this.RedirectToLocalUrl(returnUrl);

                    case SignInStatus.LockedOut:
                        var lockoutViewModel = new LockoutViewModel(this.userIdentityProvider);

                        return this.View("Lockout", lockoutViewModel);

                    case SignInStatus.RequiresVerification:
                        return this.RedirectToAction(nameof(this.SendCode), "Account", new { ReturnUrl = returnUrl, RememberBrowserCookie = false });

                    case SignInStatus.Failure:
                    default:
                        // If the user does not have an account, then prompt the user to create an account
                        var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel(this.userIdentityProvider)
                        {
                            Email = userExternalLogin.Email,
                            ReturnUrl = returnUrl,
                            LoginProvider = userExternalLogin.LinkedLogin.LoginProvider
                        };

                        return this.View(nameof(this.ExternalLoginConfirmation), externalLoginConfirmationViewModel);
                }
            }

            return this.RedirectToAction(nameof(this.Login), "Account");
        }

        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberBrowserCookie)
        {
            var userId = await this.userIdentityApplicationService.GetTwoFactorAuthenticationUserIdAsync();
            if (userId != null)
            {
                var twoFactorUserTokenProviders = await this.userIdentityApplicationService.GetValidTwoFactorAuthenticationProvidersAsync(userId);
                var twoFactorUserTokenProviderKeys = twoFactorUserTokenProviders
                    .Select(twoFactorUserTokenProviderKey => new SelectListItem { Text = twoFactorUserTokenProviderKey, Value = twoFactorUserTokenProviderKey })
                    .ToList();

                var sendCodeViewModel = new SendCodeViewModel(this.userIdentityProvider)
                {
                    TwoFactorUserTokenProviderKeys = twoFactorUserTokenProviderKeys,
                    ReturnUrl = returnUrl,
                    RememberBrowserCookie = rememberBrowserCookie
                };

                return this.View(sendCodeViewModel);
            }

            var errorViewModel = new ErrorViewModel(this.userIdentityProvider);

            return this.View("Error", errorViewModel);
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel sendCodeViewModel)
        {
            if (this.ModelState.IsValid)
            {
                if (!await this.userIdentityApplicationService.SendTwoFactorAuthenticationUserTokenAsync(sendCodeViewModel.ProviderKey))
                {
                    var errorViewModel = new ErrorViewModel(this.userIdentityProvider);

                    return this.View("Error", errorViewModel);
                }

                return this.RedirectToAction(nameof(this.VerifyCode), "Account", new { sendCodeViewModel.ProviderKey, sendCodeViewModel.ReturnUrl, sendCodeViewModel.RememberBrowserCookie });
            }

            return this.View(sendCodeViewModel);
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel externalLoginConfirmationViewModel, string returnUrl)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("Index", "Manage");
            }

            if (this.ModelState.IsValid)
            {
                var userExternalLogin = await this.userIdentityApplicationService.GetUserExternalLoginAsync();
                if (userExternalLogin == null)
                {
                    var externalLoginFailureViewModel = new ExternalLoginFailureViewModel(this.userIdentityProvider);

                    return this.View(nameof(this.ExternalLoginFailure), externalLoginFailureViewModel);
                }

                var user = new User { UserName = externalLoginConfirmationViewModel.Email, Email = externalLoginConfirmationViewModel.Email };

                var identityResult = await this.userIdentityApplicationService.CreateAsync(user);
                if (identityResult.IsValid)
                {
                    identityResult = await this.userIdentityApplicationService.CreateLoginAsync(user.Id, userExternalLogin.LinkedLogin);
                    if (identityResult.IsValid)
                    {
                        await this.userIdentityApplicationService.SignInAsync(user, false, false);

                        return this.RedirectToLocalUrl(returnUrl);
                    }
                }

                this.AddModelErrors(identityResult);
            }

            externalLoginConfirmationViewModel.ReturnUrl = returnUrl;

            return this.View(externalLoginConfirmationViewModel);
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            var externalLoginFailureViewModel = new ExternalLoginFailureViewModel(this.userIdentityProvider);

            return this.View(externalLoginFailureViewModel);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.userIdentityApplicationService.SignOut();

            return this.RedirectToAction("Index", "Home");
        }

        private void AddModelErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error);
            }
        }

        private ActionResult RedirectToLocalUrl(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}