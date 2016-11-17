namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Model;

    public interface IUserLoginRepository : IUserRepository
    {
        Task AddLoginAsync(User user, UserLinkedLogin linkedLogin);
        Task RemoveLoginAsync(User user, UserLinkedLogin linkedLogin);
        Task<IList<UserLinkedLogin>> GetLoginsAsync(User user);
        Task<User> FindAsync(UserLinkedLogin linkedLogin);
    }
}
