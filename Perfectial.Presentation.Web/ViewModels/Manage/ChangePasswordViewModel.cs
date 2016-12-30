namespace Perfectial.Presentation.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;
    using Perfectial.Presentation.Web.ViewModels;

    public class ChangePasswordViewModel : ViewModelBase
    {
        public ChangePasswordViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Change Password";

        [Required]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "ChangePasswordViewModelCurrentPassword")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ChangePasswordViewModelNewPasswordErrorMessage", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "ChangePasswordViewModelNewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "ChangePasswordViewModelConfirmPassword")]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ChangePasswordViewModelConfirmPasswordErrorMessage")]
        public string ConfirmPassword { get; set; }
    }
}