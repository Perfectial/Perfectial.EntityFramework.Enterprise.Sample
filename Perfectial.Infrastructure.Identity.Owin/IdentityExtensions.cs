namespace Perfectial.Infrastructure.Identity.Owin
{
    using System.Security.Claims;
    using System.Security.Principal;

    public static class IdentityExtensions
    {
        private const string IdentityClaimsNameType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string IdentityClaimsNameIdentifierType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public static string GetUserName(this IIdentity identity)
        {
            string userName = null;

            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                userName = claimsIdentity.FindFirst(IdentityClaimsNameType)?.Value;
            }
                
            return userName;
        }

        public static string GetUserId(this IIdentity identity)
        {
            string userId = null;

            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                userId = claimsIdentity.FindFirst(IdentityClaimsNameIdentifierType)?.Value;
            }

            return userId;
        }
    }
}
