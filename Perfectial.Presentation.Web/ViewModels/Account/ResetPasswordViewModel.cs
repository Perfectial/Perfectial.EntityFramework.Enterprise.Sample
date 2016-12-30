namespace Perfectial.Presentation.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;

    /*    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }

    }*/

    public class ResetPasswordViewModel : ViewModelBase
    {
        public ResetPasswordViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Reset Password";

        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(Resource), Name = "ResetPasswordViewModelEmail")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ResetPasswordViewModelPasswordErrorMessage", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "ResetPasswordViewModelPassword")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "ResetPasswordViewModelConfirmPassword")]
        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ResetPasswordViewModelConfirmPasswordErrorMessage")]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}
