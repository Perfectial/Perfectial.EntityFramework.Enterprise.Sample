using Perfectial.Infrastructure.Services.Base;
using Perfectial.DataAccess.Interfaces;
using System.Threading.Tasks;
using Perfectial.Infrastructure.Utils;
using Perfectial.Common;
using Perfectial.Core.Domain.Model;
using Perfectial.Infrastructure.Services.Interfaces;
using Perfectial.DataAccess.Implementation;

namespace Perfectial.Infrastructure.Services.Services
{
    public class UserService : BaseService<User, UserDto>, IUserService
    {
        #region Constants
        private const string DEF_SAVE_USER_ERROR_LOG_TEMPLATE = "Save User Failed, ID: {0}";
        #endregion

        #region Constructor
        public UserService(IDbContextScopeFactory scopeFactory) : base(scopeFactory) { }
        public UserService() : base(new DbContextScopeFactory()) { }
        #endregion

        #region Service Methods
        public async Task<bool> SaveUserAsync(UserDto userSpecification)
        {
            using (var dbScope = _dbContextScopeFactory.Create())
            {
                var entity = await dbScope.GetRepository<User>().GetByIdAsync(userSpecification.Id);
                if (entity == null)
                {
                    _logger.Inform(string.Format(
                        DEF_SAVE_USER_ERROR_LOG_TEMPLATE, userSpecification.Id));
                }
                else
                {
                    var user = Mapper.MapContents(userSpecification, entity);
                    if (user.Validate())
                    {
                        return await dbScope.SaveChangesAsync() > 0;
                    }
                }
            }
            return false;
        }

        public async Task<int> DeleteAsync(int id)
        {
            using (var dbScope = _dbContextScopeFactory.Create())
            {
                var entity = await dbScope.GetRepository<User>().GetByIdAsync(id);
                entity.Delete();
                return await dbScope.SaveChangesAsync();
            }
        }

        public async Task<int> UpdateTopScoredUser()
        {
            using (var dbScope = _dbContextScopeFactory.Create())
            {
                var entity = await dbScope.GetRepository<User>().GetByAsync(User.MostScoredUserSpecification());
                entity.Delete();

                return await dbScope.SaveChangesAsync();
            }
        }
        #endregion
    }
}
