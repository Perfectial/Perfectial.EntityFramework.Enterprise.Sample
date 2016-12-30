namespace Perfectial.Presentation.Web.Models
{

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.ViewModels;

    public class HomeIndexViewModel : ViewModelBase
    {
        public HomeIndexViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Home";
    }
}