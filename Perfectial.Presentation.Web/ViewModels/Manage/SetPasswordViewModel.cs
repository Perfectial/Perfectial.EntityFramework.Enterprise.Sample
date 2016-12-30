namespace Perfectial.Presentation.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;
    using Perfectial.Presentation.Web.ViewModels;

    public class SetPasswordViewModel : ViewModelBase
    {
        public SetPasswordViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Set Password";

        [Required]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "SetPasswordViewModelNewPasswordErrorMessage", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "SetPasswordViewModelNewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "SetPasswordViewModelConfirmPassword")]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "SetPasswordViewModelConfirmPasswordErrorMessage")]
        public string ConfirmPassword { get; set; }
    }
}