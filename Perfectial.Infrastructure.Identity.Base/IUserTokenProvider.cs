namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Model;

    public interface IUserTokenProvider
    {
        Task<string> GenerateAsync(string modifier, User user);
        Task<bool> ValidateAsync(string modifier, User user, string token);
        Task<bool> IsValidAsync(User user);
        Task NotifyAsync(User user, string token);
    }
}
