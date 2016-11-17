namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IUserSecurityStampRepository : IUserRepository
    {
        Task SetSecurityStampAsync(User user, string securityStamp);
        Task<string> GetSecurityStampAsync(User user);
    }
}
