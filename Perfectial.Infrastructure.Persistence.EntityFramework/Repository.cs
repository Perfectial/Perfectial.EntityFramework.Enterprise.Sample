namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
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

        protected DbSet<TEntity> DbSet => this.DbContext.Set<TEntity>();

        public virtual IQueryable<TEntity> GetAll()
        {
            return this.DbSet.AsQueryable();
        }

        public virtual List<TEntity> GetAllList()
        {
            return this.GetAll().ToList();
        }

        public virtual Task<List<TEntity>> GetAllListAsync()
        {
            return this.GetAll().ToListAsync();
        }

        public virtual List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).ToList();
        }

        public virtual Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).ToListAsync();
        }

        public virtual TEntity Get(TPrimaryKey id)
        {
            TEntity entity = this.FirstOrDefault(id);
            if ((object)entity == null)
            {
                throw new ArgumentException($"There is no such an entity with given primary key. Entity type: {typeof(TEntity).FullName} , primary key: {id}");
            }

            return entity;
        }

        public virtual async Task<TEntity> GetAsync(TPrimaryKey id)
        {
            TEntity entity = await this.FirstOrDefaultAsync(id);
            if ((object)entity == null)
            {
                throw new ArgumentException($"There is no such an entity with given primary key. Entity type: {typeof(TEntity).FullName} , primary key: {id}");
            }

            return entity;
        }

        public virtual TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Single(predicate);
        }

        public virtual Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().SingleAsync(predicate);
        }

        public virtual TEntity FirstOrDefault(TPrimaryKey id)
        {
            return this.GetAll().FirstOrDefault(e => e.Id.CompareTo(id) == 0);
        }

        public virtual Task<TEntity> FirstOrDefaultAsync(TPrimaryKey id)
        {
            return this.GetAll().FirstOrDefaultAsync(e => e.Id.CompareTo(id) == 0);
        }

        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().FirstOrDefault(predicate);
        }

        public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().FirstOrDefaultAsync(predicate);
        }

        public virtual void Attach(TEntity entity)
        {
            this.DbSet.Attach(entity);
        }

        public virtual TEntity Add(TEntity entity)
        {
            this.AttachNavigationProperty(entity);

            return this.DbSet.Add(entity);
        }

        public virtual Task<TEntity> AddAsync(TEntity entity)
        {
            this.AttachNavigationProperty(entity);

            return Task.FromResult(this.DbSet.Add(entity));
        }

        public virtual IEnumerable<TEntity> AddRange(IList<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.AttachNavigationProperty(entity);
            }

            return this.DbSet.AddRange(entities);
        }

        public virtual Task<IEnumerable<TEntity>> AddRangeAsync(IList<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.AttachNavigationProperty(entity);
            }

            return Task.FromResult(this.DbSet.AddRange(entities));
        }

        public virtual TEntity AddOrUpdate(TEntity entity)
        {
            if (!entity.IsTransient())
            {
                return this.Update(entity);
            }

            return this.Add(entity);
        }

        public virtual async Task<TEntity> AddOrUpdateAsync(TEntity entityToAddUpdate)
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

        public virtual TEntity Update(TEntity entity)
        {
            var entityToUpdate = this.DbSet.Find(entity.Id);
            this.DbContext.Entry(entityToUpdate).CurrentValues.SetValues(entity);

            return entityToUpdate;
        }

        public virtual Task<TEntity> UpdateAsync(TEntity entity)
        {
            var entityToUpdate = this.DbSet.Find(entity.Id);
            this.DbContext.Entry(entityToUpdate).CurrentValues.SetValues(entity);

            return Task.FromResult(entityToUpdate);
        }

        public virtual void Delete(TEntity entity)
        {
            this.DbSet.Attach(entity);
            
            this.DbSet.Remove(entity);
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            this.Delete(entity);

            return Task.FromResult(0);
        }

        public virtual void Delete(TPrimaryKey id)
        {
            var entity = this.DbSet.Find(id);

            this.Delete(entity);
        }

        public virtual Task DeleteAsync(TPrimaryKey id)
        {
            this.Delete(id);

            return Task.FromResult(0);
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            foreach (TEntity entity in this.GetAll().Where(predicate).ToList())
            {
                this.Delete(entity);
            }
        }

        public virtual Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            this.Delete(predicate);

            return Task.FromResult(0);
        }

        public virtual void DeleteRange(IList<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.DbSet.Attach(entity);
            }

            this.DbSet.RemoveRange(entities);
        }

        public virtual Task DeleteRangeAsync(IList<TEntity> entities)
        {
            this.DeleteRange(entities);

            return Task.FromResult(0);
        }

        public virtual int Count()
        {
            return this.GetAll().Count();
        }

        public virtual Task<int> CountAsync()
        {
            return this.GetAll().CountAsync();
        }

        public virtual int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).Count();
        }

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.GetAll().Where(predicate).CountAsync();
        }

        private void AttachNavigationProperty(TEntity entity)
        {
            var entityNavigationProperties = this.GetNavigationProperties<TEntity>();
            foreach (var entityNavigationProperty in entityNavigationProperties)
            {
                var entityProperty = typeof(TEntity).GetProperty(entityNavigationProperty.Name);
                var entityNavigationObject = entityProperty.GetValue(entity);

                if (entityNavigationObject != null)
                {
                    if (entityNavigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                    && entityNavigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                    {
                        var entityNavigationCollection = entityNavigationObject as IEnumerable;
                        if (entityNavigationCollection != null)
                        {
                            foreach (var entityNavigationCollectionItem in entityNavigationCollection)
                            {
                                this.AttachNavigationProperty(entity, entityNavigationCollectionItem);
                            }
                        }
                    }
                    else if ((entityNavigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One
                                || entityNavigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
                                && entityNavigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                    {
                        var entityNavigationCollection = entityNavigationObject as IEnumerable;
                        if (entityNavigationCollection != null)
                        {
                            foreach (var entityNavigationCollectionItem in entityNavigationCollection)
                            {
                                this.AttachNavigationProperty(entity, entityNavigationCollectionItem);
                            }
                        }
                    }
                    else if (entityNavigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                                && (entityNavigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One
                                || entityNavigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne))
                    {
                        this.AttachNavigationProperty(entity, entityNavigationObject);
                    }
                    else
                    {
                    }
                }
            }
        }

        private void AttachNavigationProperty(TEntity entity, object entityNavigationObject)
        {
            var entityNavigationObjectIdProperty = entityNavigationObject.GetType().GetProperty(nameof(entity.Id));
            var entityNavigationObjectId = entityNavigationObjectIdProperty.GetValue(entityNavigationObject);
            if (entityNavigationObjectId != null)
            {
                var defaultId = Activator.CreateInstance(entityNavigationObjectId.GetType());

                if (entityNavigationObjectId != defaultId)
                {
                    this.DbContext.Set(entityNavigationObject.GetType()).Attach(entityNavigationObject);
                }
            }
        }

        private IEnumerable<NavigationProperty> GetNavigationProperties<T>() where T : class
        {
            var entityType = ((IObjectContextAdapter)this.DbContext).ObjectContext.MetadataWorkspace
                               .GetItems(DataSpace.OSpace).OfType<EntityType>()
                               .FirstOrDefault(e => e.Name == typeof(T).Name);

            return entityType != null ? entityType.NavigationProperties : Enumerable.Empty<NavigationProperty>();
        }
    }
}