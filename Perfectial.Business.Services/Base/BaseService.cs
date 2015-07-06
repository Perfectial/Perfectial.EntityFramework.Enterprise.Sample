using Perfectial.Infrastructure.Utils;
using System.Threading.Tasks;
using Perfectial.DataAccess.Interfaces;
using Perfectial.Infrastructure.Utils.Logger;
using Perfectial.Core.Domain.Model;

namespace Perfectial.Infrastructure.Services.Base
{
    public abstract class BaseService<TEntity, TDto>
        where TEntity : class
        where TDto : class
    {
        #region Protected Readonly
        protected readonly IDbContextScopeFactory _dbContextScopeFactory;
        protected readonly ILogger _logger;
        #endregion

        #region Constants
        private const string DEF_GET_OBJECT_LOG_TEMPLATE = "Get action, Type: {0}, Id: {1}";
        #endregion

        #region Constructors
        public BaseService(IDbContextScopeFactory dbContextScopeFactory)
        {
            _dbContextScopeFactory = dbContextScopeFactory;
        }

        public BaseService(IDbContextScopeFactory dbContextScopeFactory, ILogger logger)
        {
            _logger = logger;
            _dbContextScopeFactory = dbContextScopeFactory;
        }
        #endregion

        #region Base Service Methods
        public async Task<TDto> Get(int id)
        {
            _logger.Inform(string.Format(DEF_GET_OBJECT_LOG_TEMPLATE, typeof(TDto).Name, id));

            using (var dbScope = _dbContextScopeFactory.CreateReadOnly())
            {
                return Mapper.Map<TDto>(await dbScope.GetRepository<User>().GetByIdAsync(id));
            }
        }
        #endregion
    }
}
