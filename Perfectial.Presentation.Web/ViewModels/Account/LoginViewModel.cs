namespace Perfectial.Presentation.Web.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Microsoft.Owin.Security;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Resources;

    public class LoginViewModel : ViewModelBase
    {
        public LoginViewModel(IIdentityProvider identityProvider)
            : base(identityProvider)
        {
            this.ExternalAuthenticationTypes = new List<AuthenticationDescription>();
        }

        public override string PageTitle => "Log in";

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "LoginViewModelEmail")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "LoginViewModelPassword")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "LoginViewModelRememberBrowserCookie")]
        public bool RememberBrowserCookie { get; set; }

        public string ReturnUrl { get; set; }

        public List<AuthenticationDescription> ExternalAuthenticationTypes { get; set; }
    }
}