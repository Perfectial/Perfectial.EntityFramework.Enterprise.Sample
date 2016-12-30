namespace Perfectial.Presentation.Web.ViewModels
{
    using Perfectial.Infrastructure.Identity.Base;

    public class LockoutViewModel : ViewModelBase
    {
        public LockoutViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Account is Locked Out";
    }
}