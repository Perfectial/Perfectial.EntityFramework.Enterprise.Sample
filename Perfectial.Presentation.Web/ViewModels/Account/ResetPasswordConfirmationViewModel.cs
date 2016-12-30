namespace Perfectial.Presentation.Web.ViewModels
{
    using Perfectial.Infrastructure.Identity.Base;

    public class ResetPasswordConfirmationViewModel : ViewModelBase
    {
        public ResetPasswordConfirmationViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Reset Password Confirmation";
    }
}