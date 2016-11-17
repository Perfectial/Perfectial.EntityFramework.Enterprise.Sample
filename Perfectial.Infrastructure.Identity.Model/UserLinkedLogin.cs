namespace Perfectial.Infrastructure.Identity.Model
{
    public class UserLinkedLogin
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }

        public UserLinkedLogin(string loginProvider, string providerKey)
        {
            this.LoginProvider = loginProvider;
            this.ProviderKey = providerKey;
        }
    }
}