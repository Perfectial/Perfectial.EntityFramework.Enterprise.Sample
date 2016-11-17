namespace Perfectial.Infrastructure.Identity.Base
{
    using System;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserTwoFactorRepository : IUserRepository
    {
        Task SetTwoFactorEnabledAsync(User user, bool enabled);
        Task<bool> GetTwoFactorEnabledAsync(User user);
    }
}