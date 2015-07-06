using Perfectial.DataAccess.DatabaseContext;
using Perfectial.DataAccess.Interfaces;
using Perfectial.DataAccess.EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Perfectial.Core.Repository;
using Perfectial.Core.Domain.Models.Base;

namespace Perfectial.DataAccess.Repository
{
    /// <summary>
    /// Generic repository implementation, in most cases you do not need to create specific repository implementation
    /// </summary>
    /// <typeparam name="TEntity">Domain entity type</typeparam>
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : AggregateRoot<TEntity>
    {
        protected readonly IAmbientDbContextLocator _ambientDbContextLocator;

        /// <summary>
        /// Override in case of using another datacontext in derived repositories
        /// </summary>
        protected virtual DataContext DataContext
        {
            get
            {
                var dbContext = _ambientDbContextLocator.Get<DataContext>();

                if (dbContext == null)
                    throw new InvalidOperationException("No ambient DbContext of type UserManagementDbContext found. This means that this repository method has been called outside of the scope of a DbContextScope. A repository must only be accessed within the scope of a DbContextScope, which takes care of creating the DbContext instances that the repositories need and making them available as ambient contexts. This is what ensures that, for any given DbContext-derived type, the same instance is used throughout the duration of a business transaction. To fix this issue, use IDbContextScopeFactory in your top-level business logic service method to create a DbContextScope that wraps the entire business transaction that your service method implements. Then access this repository within that scope. Refer to the comments in the IDbContextScope.cs file for more details.");

                return dbContext;
            }
        }

        public Repository(IAmbientDbContextLocator ambientDbContextLocator)
        {
            if (ambientDbContextLocator == null) throw new ArgumentNullException("ambientDbContextLocator");
            _ambientDbContextLocator = ambientDbContextLocator;
        }

        #region IRepository implementation

        /// <summary>
        /// Executes query first time and puts to the cache
        /// </summary>
        public async Task<TEntity> GetByIdAsync(int id)
        {
            
            var entity = await DataContext.Set<TEntity>().AsQueryable().ApplyIncludes(
                Activator.CreateInstance<TEntity>().GetRootMeassures().ToArray()).FirstOrDefaultAsync(e => e.Id == id);

            return entity;
        }

        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> wherePredicate = null,
                                        Expression<Func<TEntity, int>> orderBy = null,
                                        params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = DataContext.Set<TEntity>().AsQueryable();

            if (wherePredicate != null)
            {
                query = query.Where(wherePredicate);
            }

            if (includes != null)
            {
                query = query.ApplyIncludes(includes.ToArray());
            }

            if (orderBy != null)
            {
                query = query.OrderBy(orderBy);
            }

            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> wherePredicate, params Expression<Func<TEntity, object>>[] includes)
        {
            return await GetListAsync(wherePredicate, null, includes);
        }

        public void Add(params TEntity[] entities)
        {
            DataContext.Set<TEntity>().AddRange(entities);
        }

        public bool IsAdded(TEntity entity)
        {
            return DataContext.Entry(entity).State == EntityState.Added;
        }

        public void AttachIfDetached(params TEntity[] entities)
        {
            foreach (TEntity entity in entities)
            {
                if (entity != null && DataContext.Entry(entity).State == EntityState.Detached)
                {
                    DataContext.Set<TEntity>().Attach(entity);
                }
            }
        }

        public void AddOrUpdate(Expression<Func<TEntity, object>> identifierProperty, params TEntity[] entities)
        {
            DataContext.Set<TEntity>().AddOrUpdate(identifierProperty, entities);
        }

        public void Delete(params TEntity[] entities)
        {
            DataContext.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> wherePredicate = null)
        {
            if (wherePredicate != null)
                return await DataContext.Set<TEntity>().AsQueryable().SingleOrDefaultAsync(wherePredicate);

            return await DataContext.Set<TEntity>().AsQueryable().SingleOrDefaultAsync();
        }

        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> wherePredicate,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return await DataContext.Set<TEntity>().AsQueryable().ApplyIncludes(includes.ToArray()).SingleOrDefaultAsync(wherePredicate);
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> wherePredicate, params Expression<Func<TEntity, object>>[] includes)
        {
            return DataContext.Set<TEntity>().AsQueryable().ApplyIncludes(includes.ToArray()).SingleOrDefault(wherePredicate);
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> wherePredicate = null)
        {
            if (wherePredicate != null)
                return await DataContext.Set<TEntity>().AsQueryable().FirstOrDefaultAsync(wherePredicate);

            return await DataContext.Set<TEntity>().AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> wherePredicate)
        {
            return await DataContext.Set<TEntity>().AsQueryable().AnyAsync(wherePredicate);
        }

        #endregion


        public Task<TEntity> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes)
        {
            throw new NotImplementedException();
        }


        public Task<TEntity> GetByAsync(Expression<Func<TEntity, bool>> wherePredicate)
        {
            throw new NotImplementedException();
        }
    }
}
