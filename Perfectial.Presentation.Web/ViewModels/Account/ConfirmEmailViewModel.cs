namespace Perfectial.Presentation.Web.ViewModels
{
    using Perfectial.Infrastructure.Identity.Base;

    public class ConfirmEmailViewModel : ViewModelBase
    {
        public ConfirmEmailViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Confirm Email";
    }
}