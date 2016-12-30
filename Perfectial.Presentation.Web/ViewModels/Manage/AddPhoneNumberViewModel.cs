namespace Perfectial.Presentation.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;
    using Perfectial.Presentation.Web.ViewModels;

    public class AddPhoneNumberViewModel : ViewModelBase
    {
        public AddPhoneNumberViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Add Phone Number";

        [Required]
        [Phone]
        [Display(ResourceType = typeof(Resource), Name = "AddPhoneNumberViewModelPhoneNumber")]
        public string PhoneNumber { get; set; }
    }
}