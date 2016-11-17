namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    public interface IUserRepository : IRepository<User, string>
    {
        Task<User> FindByIdAsync(string id);
        Task<User> FindByNameAsync(string name);
        Task<User> FindByEmailAsync(string email);

        Task<IList<Claim>> GetClaimsAsync(User user);
        Task AddClaimAsync(User user, Claim claim);
        Task RemoveClaimAsync(User user, Claim claim);

        Task<IList<UserLinkedLogin>> GetLoginsAsync(User user);
        Task AddLoginAsync(User user, UserLinkedLogin linkedLogin);
        Task RemoveLoginAsync(User user, UserLinkedLogin linkedLogin);
        Task<User> FindByLoginAsync(UserLinkedLogin linkedLogin);

        Task<IList<string>> GetRolesAsync(User user);
        Task AddToRoleAsync(User user, string roleName);
        Task RemoveFromRoleAsync(User user, string roleName);
        Task<bool> IsInRoleAsync(User user, string roleName);
    }
}