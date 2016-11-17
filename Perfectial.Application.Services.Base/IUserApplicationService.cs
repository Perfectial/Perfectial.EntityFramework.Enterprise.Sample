namespace Perfectial.Application.Services.Base
{
    using System.Threading.Tasks;

    using Perfectial.Application.Model;

    public interface IUserApplicationService : IApplicationServiceBase
    {
        Task<GetUsersOutput> GetAllUsers();
    }
}
