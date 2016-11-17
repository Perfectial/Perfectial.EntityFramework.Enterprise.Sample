namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Identity.Model;

    public interface IIdentityMessageService
    {
        Task SendAsync(IdentityMessage message);
    }
}
