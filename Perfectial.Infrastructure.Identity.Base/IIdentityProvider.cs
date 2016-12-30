namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Security.Principal;

    public interface IIdentityProvider
    {
        string GetUserId(IIdentity userIdentity);
        string GetUserName(IIdentity userIdentity);
    }
}
