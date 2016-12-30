namespace Perfectial.Presentation.Web.Models
{
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.Owin.Security;

    public class ChallengeResult : HttpUnauthorizedResult
    {
        private const string XsrfProtectionKey = "XsrfId";

        public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null)
        {
        }

        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            this.LoginProvider = provider;
            this.RedirectUri = redirectUri;
            this.UserId = userId;
        }

        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }
        public string UserId { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var authenticationProperties = new AuthenticationProperties { RedirectUri = this.RedirectUri };
            if (this.UserId != null)
            {
                authenticationProperties.Dictionary[XsrfProtectionKey] = this.UserId;
            }

            context.HttpContext.GetOwinContext().Authentication.Challenge(authenticationProperties, this.LoginProvider);
        }
    }
}