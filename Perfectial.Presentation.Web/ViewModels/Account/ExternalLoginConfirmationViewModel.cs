namespace Perfectial.Presentation.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;

    public class ExternalLoginConfirmationViewModel: ViewModelBase
    {
        public ExternalLoginConfirmationViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Register";

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "ExternalLoginConfirmationViewModelEmail")]
        public string Email { get; set; }

        public string ReturnUrl { get; set; }

        public string LoginProvider { get; set; }
    }
}