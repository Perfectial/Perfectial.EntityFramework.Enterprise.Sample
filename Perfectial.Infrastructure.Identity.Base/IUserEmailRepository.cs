namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserEmailRepository : IUserRepository
    {
        Task SetEmailAsync(User user, string email);
        Task<string> GetEmailAsync(User user);
        Task<bool> GetEmailConfirmedAsync(User user);
        Task SetEmailConfirmedAsync(User user, bool confirmed);
        Task<User> FindByEmailAsync(string email);
    }
}
