namespace Perfectial.Infrastructure.Persistence.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;

    public interface IRepository<TEntity, in TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {
        /// <summary>
        /// Used to get a IQueryable that is used to retrieve entities from entire table.
        /// <see cref="T:Abp.Domain.Uow.UnitOfWorkAttribute"/> attribute must be used to be able to call this method since this method
        /// returns IQueryable and it requires open database connection to use it.
        /// </summary>
        /// <returns> IQueryable to be used to select entities from database. </returns>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns> List of all entities. </returns>
        List<TEntity> GetAllList();

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns> List of all entities. </returns>
        Task<List<TEntity>> GetAllListAsync();

        /// <summary>
        /// Gets all entities based on given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities. </param>
        /// <returns> List of all entities filtered by given <paramref name="predicate"/>. </returns>
        List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets all entities based on given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities. </param>
        /// <returns> List of all entities filtered by given <paramref name="predicate"/>. </returns>
        Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets an entity with given primary key.
        /// </summary>
        /// <param name="id"> Primary key of the entity to get. </param>
        /// <returns> Retrieved entity. </returns>
        TEntity Get(TPrimaryKey id);

        /// <summary>
        /// Gets an entity with given primary key.
        /// </summary>
        /// <param name="id"> Primary key of the entity to get. </param>
        /// <returns> Retrieved entity. </returns>
        Task<TEntity> GetAsync(TPrimaryKey id);

        /// <summary>
        /// Gets exactly one entity with given <paramref name="predicate"/>. Throws exception if no entity or more than one entity.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities.. </param>
        /// <returns> Retrieved entity. </returns>
        TEntity Single(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets exactly one entity with given <paramref name="predicate"/>. Throws exception if no entity or more than one entity.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities.. </param>
        /// <returns> Retrieved entity. </returns>
        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets an entity with given primary key or null if not found.
        /// </summary>
        /// <param name="id"> Primary key of the entity to get. </param>
        /// <returns> Retrieved entity or null. </returns>
        TEntity FirstOrDefault(TPrimaryKey id);

        /// <summary>
        /// Gets an entity with given primary key or null if not found.
        /// </summary>
        /// <param name="id"> Primary key of the entity to get. </param>
        /// <returns> Retrieved entity or null. </returns>
        Task<TEntity> FirstOrDefaultAsync(TPrimaryKey id);

        /// <summary>
        /// Gets an entity with given <paramref name="predicate"/> or null if not found.
        /// </summary>
        /// <param name="predicate"> Predicate to filter entities. </param>
        /// <returns> Retrieved entity or null. </returns>
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets an entity with given <paramref name="predicate"/> or null if not found.
        /// </summary>
        /// <param name="predicate"> Predicate to filter entities. </param>
        /// <returns> Retrieved entity or null. </returns>
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Attaches an entity.
        /// </summary>
        /// <param name="entity"> Entity to be attached. </param>
        void Attach(TEntity entity);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity"> Entity to be added. </param>
        /// <returns> Added entity. </returns>
        TEntity Add(TEntity entity);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity"> Entity to be added. </param>
        /// <returns> Added entity. </returns>
        Task<TEntity> AddAsync(TEntity entity);

        /// <summary>
        /// Adds a new entities.
        /// </summary>
        /// <param name="entities"> Entities to be added. </param>
        IEnumerable<TEntity> AddRange(IList<TEntity> entities);

        /// <summary>
        /// Adds a new entities.
        /// </summary>
        /// <param name="entities"> Entities to be added. </param>
        Task<IEnumerable<TEntity>> AddRangeAsync(IList<TEntity> entities);

        /// <summary>
        /// Adds or updates given entity depending on Id's value.
        /// </summary>
        /// <param name="entity"> Entity to be added or updated. </param>
        /// <returns> Added or updated entity. </returns>
        TEntity AddOrUpdate(TEntity entity);

        /// <summary>
        /// Adds or updates given entity depending on Id's value.
        /// </summary>
        /// <param name="entity"> Entity to be added or updated. </param>
        /// <returns> Added or updated entity. </returns>
        Task<TEntity> AddOrUpdateAsync(TEntity entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity"> Entity to be updated. </param>
        /// <returns> Added or updated entity. </returns>
        TEntity Update(TEntity entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity"> Entity to be updated. </param>
        /// <returns> Added or updated entity. </returns>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity"> Entity to be deleted. </param>
        void Delete(TEntity entity);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity"> Entity to be deleted. </param>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// Deletes an entity by primary key.
        /// </summary>
        /// <param name="id"> Primary key of the entity. </param>
        void Delete(TPrimaryKey id);

        /// <summary>
        /// Deletes an entity by primary key.
        /// </summary>
        /// <param name="id"> Primary key of the entity. </param>
        Task DeleteAsync(TPrimaryKey id);

        /// <summary>
        /// Deletes many entities by function.
        /// Notice that: All entities fit to given predicate are retrieved and deleted.
        /// This may cause major performance problems if there are too many entities with given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities. </param>
        void Delete(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Deletes many entities by function.
        /// Notice that: All entities fits to given predicate are retrieved and deleted.
        /// This may cause major performance problems if there are too many entities with given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities. </param>
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Deletes a new entities.
        /// </summary>
        /// <param name="entities"> Entities to be deleted. </param>
        void DeleteRange(IList<TEntity> entities);

        /// <summary>
        /// Deletes a new entities.
        /// </summary>
        /// <param name="entities"> Entities to be deleted. </param>
        Task DeleteRangeAsync(IList<TEntity> entities);

        /// <summary>
        /// Gets count of all entities.
        /// </summary>
        /// <returns> Count of entities. </returns>
        int Count();

        /// <summary>
        /// Gets count of all entities.
        /// </summary>
        /// <returns> Count of entities. </returns>
        Task<int> CountAsync();

        /// <summary>
        /// Gets count of all entities in this repository based on given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities. </param>
        /// <returns> Count of entities. </returns>
        int Count(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets count of all entities in this repository based on given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> A condition to filter entities. </param>
        /// <returns> Count of entities. </returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
