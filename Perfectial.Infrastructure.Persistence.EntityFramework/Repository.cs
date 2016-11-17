namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    public class Repository<TDbContext, TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>
        where TEntity : EntityBase<TPrimaryKey> where TDbContext : DbContext, IDbContext where TPrimaryKey : IComparable<TPrimaryKey>
    {
        protected readonly IAmbientDbContextLocator ambientDbContextLocator;

        public Repository(IAmbientDbContextLocator ambientDbContextLocator)
        {
            if (ambientDbContextLocator == null)
            {
                throw new ArgumentNullException(nameof(ambientDbContextLocator));
            }

            this.ambientDbContextLocator = ambientDbContextLocator;
        }

        protected virtual TDbContext DbContext
        {
            get
            {
                var dbContext = this.ambientDbContextLocator.Get<TDbContext>();
                if (dbContext == null)
                {
                    throw new InvalidOperationException("No ambient DbContext of type TDbContext found. This means that this repository method has been called outside of the scope of a DbContextScope. A repository must only be accessed within the scope of a DbContextScope, which takes care of creating the DbContext instances that the repositories need and making them available as ambient contexts. This is what ensures that, for any given DbContext-derived type, the same instance is used throughout the duration of a business transaction. To fix this issue, use IDbContextScopeFactory in your top-level business logic service method to create a DbContextScope that wraps the entire business transaction that your service method implements. Then access this repository within that scope. Refer to the comments in the IDbContextScope.cs file for more details.");
                }

                return dbContext;
            }
        }

        private DbSet<TEntity> DbSet => this.DbContext.Set<TEntity>();

        public IQueryable<TEntity> GetAll()
        {
            return this.DbSet.AsQueryable();
        }

        public List<TEntity> GetAllList()
        {
            return this.GetAll().ToList();
        }

        public Task<List<TEntity>> GetAllListAsync()
        {
            return this.GetAll().ToListAsync();
        }

        public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).ToList();
        }

        public Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).ToListAsync();
        }

        public TEntity Get(TPrimaryKey id)
        {
            TEntity entity = this.FirstOrDefault(id);
            if ((object)entity == null)
            {
                throw new ArgumentException($"There is no such an entity with given primary key. Entity type: {typeof(TEntity).FullName} , primary key: {id}");
            }

            return entity;
        }

        public async Task<TEntity> GetAsync(TPrimaryKey id)
        {
            TEntity entity = await this.FirstOrDefaultAsync(id);
            if ((object)entity == null)
            {
                throw new ArgumentException($"There is no such an entity with given primary key. Entity type: {typeof(TEntity).FullName} , primary key: {id}");
            }

            return entity;
        }

        public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Single(predicate);
        }

        public Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().SingleAsync(predicate);
        }

        public TEntity FirstOrDefault(TPrimaryKey id)
        {
            return this.GetAll().FirstOrDefault(e => e.Id.CompareTo(id) == 0);
        }

        public Task<TEntity> FirstOrDefaultAsync(TPrimaryKey id)
        {
            return this.GetAll().FirstOrDefaultAsync(e => e.Id.CompareTo(id) == 0);
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().FirstOrDefault(predicate);
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().FirstOrDefaultAsync(predicate);
        }

        public void Attach(TEntity entity)
        {
            this.DbSet.Attach(entity);
        }

        public TEntity Add(TEntity entity)
        {
            this.DbContext.Entry(entity).State = EntityState.Added;

            return this.DbSet.Add(entity);
        }

        public Task<TEntity> AddAsync(TEntity entity)
        {
            this.DbContext.Entry(entity).State = EntityState.Added;

            return Task.FromResult(this.DbSet.Add(entity));
        }

        public IEnumerable<TEntity> AddRange(IList<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.DbContext.Entry(entity).State = EntityState.Added;
            }

            return this.DbSet.AddRange(entities);
        }

        public Task<IEnumerable<TEntity>> AddRangeAsync(IList<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.DbContext.Entry(entity).State = EntityState.Added;
            }

            return Task.FromResult(this.DbSet.AddRange(entities));
        }

        public TEntity AddOrUpdate(TEntity entity)
        {
            if (!entity.IsTransient())
            {
                return this.Update(entity);
            }

            return this.Add(entity);
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entityToAddUpdate)
        {
            TEntity entity;
            if (!entityToAddUpdate.IsTransient())
            {
                entity = await this.UpdateAsync(entityToAddUpdate);
            }
            else
            {
                entity = await this.AddAsync(entityToAddUpdate);
            }

            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            this.Attach(entity);

            this.DbContext.Entry(entity).State = EntityState.Modified;

            return entity;
        }

        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            this.Attach(entity);

            this.DbContext.Entry(entity).State = EntityState.Modified;

            return Task.FromResult(entity);
        }

        public void Delete(TEntity entity)
        {
            this.DbSet.Attach(entity);
            this.DbContext.Entry(entity).State = EntityState.Deleted;

            this.DbSet.Remove(entity);
        }

        public Task DeleteAsync(TEntity entity)
        {
            this.Delete(entity);

            return Task.FromResult(0);
        }

        public void Delete(TPrimaryKey id)
        {
            var entity = this.DbSet.Find(id);

            this.Delete(entity);
        }

        public Task DeleteAsync(TPrimaryKey id)
        {
            this.Delete(id);

            return Task.FromResult(0);
        }

        public void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            foreach (TEntity entity in this.GetAll().Where(predicate).ToList())
            {
                this.Delete(entity);
            }
        }

        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            this.Delete(predicate);

            return Task.FromResult(0);
        }

        public void DeleteRange(IList<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.DbSet.Attach(entity);
                this.DbContext.Entry(entity).State = EntityState.Deleted;
            }

            this.DbSet.RemoveRange(entities);
        }

        public Task DeleteRangeAsync(IList<TEntity> entities)
        {
            this.DeleteRange(entities);

            return Task.FromResult(0);
        }

        public int Count()
        {
            return this.GetAll().Count();
        }

        public Task<int> CountAsync()
        {
            return this.GetAll().CountAsync();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).Count();
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).CountAsync();
        }
    }
}