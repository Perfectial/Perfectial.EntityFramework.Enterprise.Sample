namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserPhoneNumberStore : IUserStore
    {
        void SetPhoneNumber(User user, string phoneNumber);
        Task SetPhoneNumberAsync(User user, string phoneNumber);
        string GetPhoneNumber(User user);
        Task<string> GetPhoneNumberAsync(User user);
        bool GetPhoneNumberConfirmed(User user);
        Task<bool> GetPhoneNumberConfirmedAsync(User user);
        void SetPhoneNumberConfirmed(User user, bool confirmed);
        Task SetPhoneNumberConfirmedAsync(User user, bool confirmed);
    }
}
