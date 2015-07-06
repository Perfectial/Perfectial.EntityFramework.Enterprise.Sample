using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.Core.Repository.Interfaces
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);

        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> wherePredicate = null,
                                         Expression<Func<TEntity, int>> orderBy = null,
                                         params Expression<Func<TEntity, object>>[] includes);


        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> wherePredicate,
                                         params Expression<Func<TEntity, object>>[] includes);

        void Add(params TEntity[] entities);

        bool IsAdded(TEntity entity);

        void AttachIfDetached(params TEntity[] entities);

        void AddOrUpdate(Expression<Func<TEntity, object>> identifierProperty, params TEntity[] entities);

        void Delete(params TEntity[] entities);

        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> wherePredicate,
                                 params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> wherePredicate = null);

        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> wherePredicate = null);

        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> wherePredicate,
                                         params Expression<Func<TEntity, object>>[] includes);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> wherePredicate);
    }
}
