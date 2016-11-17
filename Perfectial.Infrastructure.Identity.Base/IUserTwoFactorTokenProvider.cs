namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Model;

    public interface IUserTokenTwoFactorProvider
    {
        Task RegisterTwoFactorUserTokenProviderAsync(string twoFactorProviderKey, IUserTokenProvider userTokenProvider);
        Task<IList<string>> GetValidTwoFactorUserTokenProvidersAsync(User user);
        Task<string> GenerateTwoFactorUserTokenAsync(string twoFactorProviderKey, User user);
        Task<bool> ValidateTwoFactorUserTokenAsync(string twoFactorProviderKey, User user, string token);
        Task NotifyTwoFactorUserTokenAsync(string twoFactorProviderKey, User user, string token);
    }
}
