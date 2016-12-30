namespace Perfectial.Presentation.Web.Models
{
    using System.Collections.Generic;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Presentation.Web.ViewModels;

    public class IndexViewModel : ViewModelBase
    {
        public IndexViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Manage Accounts";

        public string PhoneNumber { get; set; }

        public bool HasPassword { get; set; }
        public bool TwoFactorUserTokenAuthenticationIsEnabled { get; set; }
        public bool BrowserRemembered { get; set; }
        public string StatusMessage { get; set; }

        public IList<UserLinkedLogin> UserLinkedLogins { get; set; }
    }
}