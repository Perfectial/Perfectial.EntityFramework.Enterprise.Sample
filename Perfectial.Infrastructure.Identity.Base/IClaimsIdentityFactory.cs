namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IClaimsIdentityFactory
    {
        Task<ClaimsIdentity> CreateAsync(User user, string authenticationType, IEnumerable<string> userRoles, IEnumerable<Claim> userClaims);
    }
}
