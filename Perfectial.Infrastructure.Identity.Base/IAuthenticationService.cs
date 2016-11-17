namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Model;

    public interface IAuthenticationService
    {
        int GenerateCode(SecurityToken securityToken, string modifier = null);
        bool ValidateCode(SecurityToken securityToken, int code, string modifier = null);
    }
}
