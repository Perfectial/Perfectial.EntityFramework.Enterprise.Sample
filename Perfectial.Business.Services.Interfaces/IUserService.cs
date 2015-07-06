using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using Perfectial.Common;
using Perfectial.Core.Domain.Model;

namespace Perfectial.Infrastructure.Services.Interfaces
{
    public interface IUserService : IEntityService<User>
    {
        Task<bool> SaveUserAsync(UserDto userSpecification);
    }
}
