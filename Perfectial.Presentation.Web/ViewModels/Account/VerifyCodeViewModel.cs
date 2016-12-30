namespace Perfectial.Presentation.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;

    public class VerifyCodeViewModel: ViewModelBase
    {
        public VerifyCodeViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
        }

        public override string PageTitle => "Verify Code";

        [Required]
        public string ProviderKey { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "VerifyCodeViewModelCode")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "VerifyCodeViewModelRememberBrowser")]
        public bool RememberBrowser { get; set; }

        public bool RememberBrowserCookie { get; set; }
    }
}