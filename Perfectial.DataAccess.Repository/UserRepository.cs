using Perfectial.DataAccess.Repository;
using Perfectial.DataAccess.Interfaces;
using Perfectial.Core.Domain.Model;

namespace EntityFramework.DataAccess.Repository
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(IAmbientDbContextLocator ambientContextLocator)
            : base(ambientContextLocator)
        {}

        //TODO: custom DB methods here
    }
}
