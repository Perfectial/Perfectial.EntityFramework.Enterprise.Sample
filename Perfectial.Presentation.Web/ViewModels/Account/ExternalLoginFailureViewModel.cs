namespace Perfectial.Presentation.Web.ViewModels
{
    using Perfectial.Infrastructure.Identity.Base;

    public class ExternalLoginFailureViewModel : ViewModelBase
    {
        public ExternalLoginFailureViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Login Failure";
    }
}