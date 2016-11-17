namespace Perfectial.Infrastructure.Identity.Base
{
    using System;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserLockoutRepository : IUserRepository
    {
        Task<DateTimeOffset> GetLockoutEndDateAsync(User user);
        Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd);
        Task<int> IncrementAccessFailedCountAsync(User user);
        Task ResetAccessFailedCountAsync(User user);
        Task<int> GetAccessFailedCountAsync(User user);
        Task<bool> GetLockoutEnabledAsync(User user);
        Task SetLockoutEnabledAsync(User user, bool enabled);
    }
}