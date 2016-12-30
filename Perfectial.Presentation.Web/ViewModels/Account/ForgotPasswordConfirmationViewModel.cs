namespace Perfectial.Presentation.Web.ViewModels
{
    using Perfectial.Infrastructure.Identity.Base;

    public class ForgotPasswordConfirmationViewModel : ViewModelBase
    {
        public ForgotPasswordConfirmationViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Forgot Password Confirmation";
    }
}