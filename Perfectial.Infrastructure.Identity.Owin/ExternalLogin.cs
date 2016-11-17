namespace Perfectial.Infrastructure.Identity.Owin
{
    using System.Security.Claims;

    using Perfectial.Infrastructure.Identity.Model;

    public class ExternalLogin
    {
        public UserLinkedLogin LinkedLogin { get; set; }
        public string DefaultUserName { get; set; }
        public string Email { get; set; }
        public ClaimsIdentity ExternalIdentity { get; set; }
    }
}