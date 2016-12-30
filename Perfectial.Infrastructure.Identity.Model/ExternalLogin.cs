namespace Perfectial.Infrastructure.Identity.Model
{
    using System.Security.Claims;

    public class UserExternalLogin
    {
        public UserLinkedLogin LinkedLogin { get; set; }
        public string DefaultUserName { get; set; }
        public string Email { get; set; }
        public ClaimsIdentity ExternalIdentity { get; set; }
    }
}