namespace Perfectial.Presentation.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Perfectial.Application.Services.Base;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Presentation.Web.Models;
    using Perfectial.Presentation.Web.Resources;
    using Perfectial.Presentation.Web.ViewModels;

    using IdentityMessage = Perfectial.Infrastructure.Identity.Model.IdentityMessage;
    using IdentityResult = Perfectial.Infrastructure.Identity.Model.IdentityResult;
    using IIdentityMessageService = Perfectial.Infrastructure.Identity.Base.IIdentityMessageService;

    [Authorize]
    public class ManageController : Controller
    {
        private const string XsrfKey = "XsrfId";

        private readonly IUserIdentityApplicationService userIdentityApplicationService;
        private readonly IIdentityProvider userIdentityProvider;
        private readonly IIdentityMessageService userIdentityMessageService;

        public ManageController(
            IUserIdentityApplicationService userIdentityApplicationService,
            IIdentityProvider userIdentityProvider,
            IIdentityMessageService userIdentityMessageService)
        {
            this.userIdentityApplicationService = userIdentityApplicationService;
            this.userIdentityProvider = userIdentityProvider;
            this.userIdentityMessageService = userIdentityMessageService;
        }

        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageControllerIndexViewMessageType? messageType)
        {
            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

            var indexViewModel = new IndexViewModel(this.userIdentityProvider)
            {
                HasPassword = await this.UserHasPasswordAsync(),
                PhoneNumber = await this.userIdentityApplicationService.GetPhoneNumberAsync(userId),
                TwoFactorUserTokenAuthenticationIsEnabled = await this.userIdentityApplicationService.GetTwoFactorUserTokenAuthenticationIsEnabledAsync(userId),
                UserLinkedLogins = await this.userIdentityApplicationService.GetLoginsAsync(userId),
                BrowserRemembered = await this.userIdentityApplicationService.AuthenticateIdentityAsync(userId, AuthenticationType.TwoFactorRememberBrowserCookie),
                StatusMessage = this.GetIndexViewStatusMessage(messageType)
            };

            return this.View(indexViewModel);
        }

        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            var addPhoneNumberViewModel = new AddPhoneNumberViewModel(this.userIdentityProvider);

            return this.View(addPhoneNumberViewModel);
        }

        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel addPhoneNumberViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

                var confirmationToken = await this.userIdentityApplicationService.GeneratePhoneNumberConfirmationTokenAsync(userId, addPhoneNumberViewModel.PhoneNumber);
                var identityMessage = new IdentityMessage
                {
                    Destination = addPhoneNumberViewModel.PhoneNumber,
                    Body = "Your security code is: " + confirmationToken
                };

                await this.userIdentityMessageService.SendAsync(identityMessage);

                return this.RedirectToAction(nameof(this.VerifyPhoneNumber), new { addPhoneNumberViewModel.PhoneNumber });
            }

            return this.View(addPhoneNumberViewModel);
        }

        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var userId = this.userIdentityProvider.GetUserId(this.User.Identity);
                // TODO: Verify how it works.
                await this.userIdentityApplicationService.GeneratePhoneNumberConfirmationTokenAsync(userId, phoneNumber);

                var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel(this.userIdentityProvider) { PhoneNumber = phoneNumber };

                return this.View(verifyPhoneNumberViewModel);
            }

            var errorViewModel = new ErrorViewModel(this.userIdentityProvider);

            return this.View("Error", errorViewModel);
        }

        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel verifyPhoneNumberViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

                var identityResult = await this.userIdentityApplicationService.UpdatePhoneNumberAsync(userId, verifyPhoneNumberViewModel.PhoneNumber, verifyPhoneNumberViewModel.Code);
                if (identityResult.IsValid)
                {
                    var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await this.userIdentityApplicationService.SignInAsync(user, false, false);
                    }

                    return this.RedirectToAction(nameof(this.Index), new { MessageType = ManageControllerIndexViewMessageType.AddPhoneSuccess });
                }

                this.ModelState.AddModelError(string.Empty, Resource.ManageControllerVerifyPhoneNumberErrorMessage);
            }

            return this.View(verifyPhoneNumberViewModel);
        }

        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

            var identityResult = await this.userIdentityApplicationService.SetPhoneNumberAsync(userId, null);
            if (identityResult.IsValid)
            {
                var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
                if (user != null)
                {
                    await this.userIdentityApplicationService.SignInAsync(user, false, false);
                }

                return this.RedirectToAction(nameof(this.Index), new { Message = ManageControllerIndexViewMessageType.RemovePhoneSuccess });
            }

            return this.RedirectToAction(nameof(this.Index), new { Message = ManageControllerIndexViewMessageType.Error });
        }

        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            var changePasswordViewModel = new ChangePasswordViewModel(this.userIdentityProvider);

            return this.View(changePasswordViewModel);
        }

        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

                var identityResult = await this.userIdentityApplicationService.UpdatePasswordAsync(userId, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);
                if (identityResult.IsValid)
                {
                    var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await this.userIdentityApplicationService.SignInAsync(user, false, false);
                    }

                    return this.RedirectToAction(nameof(this.Index), new { Message = ManageControllerIndexViewMessageType.ChangePasswordSuccess });
                }

                this.AddModelErrors(identityResult);
            }

            return this.View(changePasswordViewModel);
        }

        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            var setPasswordViewModel = new SetPasswordViewModel(this.userIdentityProvider);

            return this.View(setPasswordViewModel);
        }

        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel setPasswordViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

                var identityResult = await this.userIdentityApplicationService.SetPasswordAsync(userId, setPasswordViewModel.NewPassword);
                if (identityResult.IsValid)
                {
                    var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await this.userIdentityApplicationService.SignInAsync(user, false, false);
                    }

                    return this.RedirectToAction(nameof(this.Index), new { Message = ManageControllerIndexViewMessageType.SetPasswordSuccess });
                }

                this.AddModelErrors(identityResult);
            }

            return this.View(setPasswordViewModel);
        }

        // GET: /Manage/ManageExternalLogins
        public async Task<ActionResult> ManageExternalLogins(ManageControllerIndexViewMessageType? messageType)
        {
            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

            var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
            if (user == null)
            {
                var errorViewModel = new ErrorViewModel(this.userIdentityProvider);

                return this.View("Error", errorViewModel);
            }

            var userLogins = await this.userIdentityApplicationService.GetLoginsAsync(userId);
            var externalAuthenticationTypes = this.userIdentityApplicationService.GetExternalAuthenticationTypes().ToList();
            var externalUserLogins = externalAuthenticationTypes
                .Where(authenticationType => userLogins.All(userLogin => authenticationType.AuthenticationType != userLogin.LoginProvider)).ToList();
            var mumberOfExternalAuthenticationTypes = externalAuthenticationTypes.Count();

            var manageExternalLoginsViewModel = new ManageExternalLoginsViewModel(this.userIdentityProvider)
            {
                UserLinkedLogins = userLogins,
                ExternalAuthenticationTypes = externalUserLogins,
                NumberOfExternalAuthenticationTypes = mumberOfExternalAuthenticationTypes,
                StatusMessage = this.GetManageLoginsViewStatusMessage(messageType),
                ShowRemoveExternalAccountButton = user.PasswordHash != null || userLogins.Count > 1
            };

            return this.View(manageExternalLoginsViewModel);
        }

        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string providerKey)
        {
            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);
            var challengeResult = new ChallengeResult(providerKey, this.Url.Action(nameof(this.LinkLoginCallback), "Manage"), userId);

            return challengeResult;
        }

        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

            var userExternalLogin = await this.userIdentityApplicationService.GetUserExternalLoginAsync(XsrfKey, userId);
            if (userExternalLogin != null)
            {
                var identityResult = await this.userIdentityApplicationService.CreateLoginAsync(userId, userExternalLogin.LinkedLogin);
                if (identityResult.IsValid)
                {

                    this.RedirectToAction(nameof(this.ManageExternalLogins));
                }
            }

            return this.RedirectToAction(nameof(this.ManageExternalLogins), new { Message = ManageControllerIndexViewMessageType.Error });
        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageControllerIndexViewMessageType? messageType;

            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);
            var userLinkedLogin = new UserLinkedLogin(loginProvider, providerKey);

            var identityResult = await this.userIdentityApplicationService.DeleteLoginAsync(userId, userLinkedLogin);
            if (identityResult.IsValid)
            {
                var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
                if (user != null)
                {
                    await this.userIdentityApplicationService.SignInAsync(user, false, false);
                }

                messageType = ManageControllerIndexViewMessageType.RemoveLoginSuccess;
            }
            else
            {
                messageType = ManageControllerIndexViewMessageType.Error;
            }

            return this.RedirectToAction(nameof(this.ManageExternalLogins), new { MessageType = messageType });
        }

        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

            await this.userIdentityApplicationService.SetTwoFactorUserTokenAuthenticationEnabledAsync(userId, true);

            var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
            if (user != null)
            {
                await this.userIdentityApplicationService.SignInAsync(user, false, false);
            }

            return this.RedirectToAction(nameof(this.Index), "Manage");
        }

        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            var userId = this.userIdentityProvider.GetUserId(this.User.Identity);

            await this.userIdentityApplicationService.SetTwoFactorUserTokenAuthenticationEnabledAsync(userId, false);

            var user = await this.userIdentityApplicationService.FindByIdAsync(userId);
            if (user != null)
            {
                await this.userIdentityApplicationService.SignInAsync(user, false, false);
            }

            return this.RedirectToAction(nameof(this.Index), "Manage");
        }

        private void AddModelErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error);
            }
        }

        private async Task<bool> UserHasPasswordAsync()
        {
            var user = await this.userIdentityApplicationService.FindByIdAsync(this.userIdentityProvider.GetUserId(this.User.Identity));

            return user?.PasswordHash != null;
        }

        private string GetIndexViewStatusMessage(ManageControllerIndexViewMessageType? messageType)
        {
            // TODO: Rework with Enumeration Display name.
            string indexViewStatusMessage = messageType == ManageControllerIndexViewMessageType.ChangePasswordSuccess
                                        ? "Your password has been changed."
                                        : messageType == ManageControllerIndexViewMessageType.SetPasswordSuccess
                                              ? "Your password has been set."
                                              : messageType == ManageControllerIndexViewMessageType.SetTwoFactorSuccess
                                                    ? "Your two-factor authentication provider has been set."
                                                    : messageType == ManageControllerIndexViewMessageType.Error
                                                          ? "An error has occurred."
                                                          : messageType == ManageControllerIndexViewMessageType.AddPhoneSuccess
                                                                ? "Your phone number was added."
                                                                : messageType == ManageControllerIndexViewMessageType.RemovePhoneSuccess
                                                                      ? "Your phone number was removed."
                                                                      : string.Empty;

            return indexViewStatusMessage;
        }

        private string GetManageLoginsViewStatusMessage(ManageControllerIndexViewMessageType? messageType)
        {
            // TODO: Rework with Enumeration Display name.
            string indexViewStatusMessage = messageType == ManageControllerIndexViewMessageType.RemoveLoginSuccess
                                        ? "The external login was removed."
                                        : messageType == ManageControllerIndexViewMessageType.Error
                                            ? "An error has occurred." : string.Empty;

            return indexViewStatusMessage;
        }

        public enum ManageControllerIndexViewMessageType
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
    }
}