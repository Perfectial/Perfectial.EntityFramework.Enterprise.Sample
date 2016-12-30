namespace Perfectial.Presentation.Web.ViewModels
{
    using System.Web;

    using Perfectial.Infrastructure.Identity.Base;

    public abstract class ViewModelBase
    {
        private readonly IIdentityProvider identityProvider;

        protected ViewModelBase(IIdentityProvider identityProvider)
        {
            this.identityProvider = identityProvider;
        }

        public abstract string PageTitle { get; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }

        public string UserId => this.identityProvider.GetUserId(HttpContext.Current.User.Identity);
        public string UserName => this.identityProvider.GetUserName(HttpContext.Current.User.Identity);
    }
}