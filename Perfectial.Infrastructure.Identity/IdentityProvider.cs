namespace Perfectial.Infrastructure.Identity
{
    using System.Security.Claims;
    using System.Security.Principal;

    using Perfectial.Infrastructure.Identity.Base;

    public class IdentityProvider : IIdentityProvider
    {
        private const string IdentityClaimsNameType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string IdentityClaimsNameIdentifierType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public string GetUserId(IIdentity userIdentity)
        {
            string userId = string.Empty;

            var claimsIdentity = userIdentity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                userId = claimsIdentity.FindFirst(IdentityClaimsNameIdentifierType)?.Value;
            }

            return userId;
        }

        public string GetUserName(IIdentity userIdentity)
        {
            string userName = string.Empty;

            var claimsIdentity = userIdentity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                userName = claimsIdentity.FindFirst(IdentityClaimsNameType)?.Value;
            }

            return userName;
        }
    }
}
