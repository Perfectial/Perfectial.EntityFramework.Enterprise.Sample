namespace Perfectial.Presentation.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;

    public class RegisterViewModel : ViewModelBase
    {
        public RegisterViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Log In";

        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(Resource), Name = "RegisterViewModelEmail")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RegisterViewModelPasswordErrorMessage", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "RegisterViewModelPassword")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "RegisterViewModelConfirmPassword")]
        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RegisterViewModelConfirmPasswordErrorMessage")]
        public string ConfirmPassword { get; set; }
    }
}