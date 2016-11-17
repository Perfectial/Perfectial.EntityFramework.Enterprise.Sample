namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserRoleRepository : IUserRepository
    {
        Task AddToRoleAsync(User user, string roleName);
        Task RemoveFromRoleAsync(User user, string roleName);
        Task<IList<string>> GetRolesAsync(User user);
        Task<bool> IsInRoleAsync(User user, string roleName);
    }
}
