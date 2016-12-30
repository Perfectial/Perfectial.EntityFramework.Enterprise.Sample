namespace Perfectial.Presentation.Web.Models
{
    using System.Collections.Generic;

    using Microsoft.Owin.Security;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Presentation.Web.ViewModels;

    public class ManageExternalLoginsViewModel : ViewModelBase
    {
        public ManageExternalLoginsViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Manage External Logins";

        public int NumberOfExternalAuthenticationTypes { get; set; }
        public string StatusMessage { get; set; }
        public bool ShowRemoveExternalAccountButton { get; set; }


        public IList<UserLinkedLogin> UserLinkedLogins { get; set; }
        public IList<AuthenticationDescription> ExternalAuthenticationTypes { get; set; }
    }
}