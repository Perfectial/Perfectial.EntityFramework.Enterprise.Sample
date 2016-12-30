namespace Perfectial.Presentation.Web.ViewModels
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Perfectial.Infrastructure.Identity.Base;

    public class SendCodeViewModel: ViewModelBase
    {
        public SendCodeViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Send Code";

        public string ProviderKey { get; set; }
        public ICollection<SelectListItem> TwoFactorUserTokenProviderKeys { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberBrowserCookie { get; set; }
    }
}