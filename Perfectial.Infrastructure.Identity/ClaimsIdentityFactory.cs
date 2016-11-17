using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Perfectial.Infrastructure.Identity
{
    using System.Security.Claims;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;

    public class ClaimsIdentityFactory : IClaimsIdentityFactory
    {
        private const string IdentityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
        private const string IdentityRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        private const string IdentityUserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string IdentityUserNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string IdentitySecurityStampClaimType = "AspNet.Identity.SecurityStamp";

        private const string DefaultClaimValueType = "http://www.w3.org/2001/XMLSchema#string";
        private const string DefaultIdentityProviderClaimValue = "ASP.NET Identity";

        public string RoleClaimType { get; set; }
        public string UserNameClaimType { get; set; }
        public string UserIdClaimType { get; set; }
        public string SecurityStampClaimType { get; set; }

        public ClaimsIdentityFactory()
        {
            this.RoleClaimType = IdentityRoleClaimType;
            this.UserIdClaimType = IdentityUserIdClaimType;
            this.UserNameClaimType = IdentityUserNameClaimType;
            this.SecurityStampClaimType = IdentitySecurityStampClaimType;
        }

        public virtual Task<ClaimsIdentity> CreateAsync(User user, string authenticationType, IEnumerable<string> userRoles, IEnumerable<Claim> userClaims)
        {
            var claimsIdentity = new ClaimsIdentity(authenticationType, this.UserNameClaimType, this.RoleClaimType);
            claimsIdentity.AddClaim(new Claim(this.UserIdClaimType, user.Id, DefaultClaimValueType));
            claimsIdentity.AddClaim(new Claim(this.UserNameClaimType, user.UserName, DefaultClaimValueType));
            claimsIdentity.AddClaim(new Claim(IdentityProviderClaimType, DefaultIdentityProviderClaimValue, DefaultClaimValueType));

            // TODO: Support Security Stamp.
            var claim = new Claim(this.SecurityStampClaimType, user.SecurityStamp);
            claimsIdentity.AddClaim(claim);

            // TODO: Support User Roles, Ensure Roles are loaded.
            if (userRoles != null)
            {
                foreach (var userRole in userRoles)
                {
                    claimsIdentity.AddClaim(new Claim(this.RoleClaimType, userRole, DefaultClaimValueType));
                }
            }

            // TODO: Support User Claims.
            if (userClaims != null)
            {
                foreach (var userClaim in userClaims)
                {
                    claimsIdentity.AddClaim(userClaim);
                }
            }

            return Task.FromResult(claimsIdentity);
        }
    }
}
