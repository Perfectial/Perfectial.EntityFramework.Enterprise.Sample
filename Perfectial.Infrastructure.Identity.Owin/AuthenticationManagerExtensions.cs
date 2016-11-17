namespace Perfectial.Infrastructure.Identity.Owin
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security;

    using Perfectial.Infrastructure.Identity.Model;

    public static class AuthenticationManagerExtensions
    {
        private const string IdentityClaimsNameidentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string IdentityClaimsEmailAddressIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        public static IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes(this IAuthenticationManager manager)
        {
            return manager.GetAuthenticationTypes(authenticationDescription =>
                {
                    if (authenticationDescription.Properties != null)
                    {
                        return authenticationDescription.Properties.ContainsKey("Caption");
                    }

                    return false;
                });
        }

        public static async Task<ExternalLogin> GetExternalLoginAsync(this IAuthenticationManager manager)
        {
            var authenticateResult = await manager.AuthenticateAsync(AuthenticationType.ExternalCookie);

            return GetExternalLogin(authenticateResult);
        }

        public static async Task<ExternalLogin> GetExternalLoginAsync(this IAuthenticationManager manager, string xsrfKey, string expectedValue)
        {
            var authenticateResult = await manager.AuthenticateAsync(AuthenticationType.ExternalCookie);

            return authenticateResult?.Properties?.Dictionary == null 
                || !authenticateResult.Properties.Dictionary.ContainsKey(xsrfKey) 
                || authenticateResult.Properties.Dictionary[xsrfKey] != expectedValue ? 
                    null : 
                    GetExternalLogin(authenticateResult);
        }

        public static async Task<bool> TwoFactorBrowserRememberedAsync(this IAuthenticationManager manager, string userId)
        {
            var authenticateResult = await manager.AuthenticateAsync(AuthenticationType.TwoFactorRememberBrowserCookie);

            return authenticateResult?.Identity != null && authenticateResult.Identity.GetUserId() == userId;
        }

        public static ClaimsIdentity CreateTwoFactorRememberBrowserIdentity(this IAuthenticationManager manager, string userId)
        {
            var claimsIdentity = new ClaimsIdentity(AuthenticationType.TwoFactorRememberBrowserCookie);
            claimsIdentity.AddClaim(new Claim(IdentityClaimsNameidentifier, userId));

            return claimsIdentity;
        }

        private static ExternalLogin GetExternalLogin(AuthenticateResult result)
        {
            ExternalLogin externalLogin = null;

            Claim firstClaim = result?.Identity?.FindFirst(IdentityClaimsNameidentifier);
            if (firstClaim != null)
            {
                string identityName = result.Identity.Name;
                identityName = identityName?.Replace(" ", string.Empty);

                string emailAddress = result.Identity.FindFirst(IdentityClaimsEmailAddressIdentifier)?.Value;
                externalLogin = new ExternalLogin
                {
                    ExternalIdentity = result.Identity,
                    LinkedLogin = new UserLinkedLogin(firstClaim.Issuer, firstClaim.Value),
                    DefaultUserName = identityName,
                    Email = emailAddress
                };
            }

            return externalLogin;
        }
    }
}
