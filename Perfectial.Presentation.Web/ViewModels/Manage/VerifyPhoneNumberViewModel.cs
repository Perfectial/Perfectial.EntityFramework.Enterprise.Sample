namespace Perfectial.Presentation.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;
    using Perfectial.Presentation.Web.ViewModels;

    public class VerifyPhoneNumberViewModel : ViewModelBase
    {
        public VerifyPhoneNumberViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Verify Phone Number";

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "VerifyPhoneNumberViewModelCode")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(ResourceType = typeof(Resource), Name = "VerifyPhoneNumberViewModelPhoneNumber")]
        public string PhoneNumber { get; set; }
    }
}