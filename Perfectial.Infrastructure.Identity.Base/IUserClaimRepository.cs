namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserClaimRepository : IUserRepository
    {
        Task<IList<Claim>> GetClaimsAsync(User user);
        Task AddClaimAsync(User user, Claim claim);
        Task RemoveClaimAsync(User user, Claim claim);
    }
}
