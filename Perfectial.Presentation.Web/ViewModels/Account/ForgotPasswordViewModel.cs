namespace Perfectial.Presentation.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;

    public class ForgotPasswordViewModel : ViewModelBase
    {
        public ForgotPasswordViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Forgot Your Password?";

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "ForgotPasswordViewModelEmail")]
        public string Email { get; set; }
    }
}