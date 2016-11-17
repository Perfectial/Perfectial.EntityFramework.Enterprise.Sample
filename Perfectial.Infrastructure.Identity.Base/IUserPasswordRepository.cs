namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserPasswordRepository : IUserRepository
    {
        Task SetPasswordHashAsync(User user, string passwordHash);
        Task<string> GetPasswordHashAsync(User user);
        Task<bool> HasPasswordAsync(User user);
    }
}
