using Perfectial.DataAccess.Interfaces;
using System;
using System.Linq;
using System.Collections;
using System.Data.Entity;
using System.Reflection;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using Perfectial.DataAccess.Interfaces.Enums;
using Perfectial.DataAccess.EntityFramework;
using LaunchPoint.DataAccess;

namespace Perfectial.DataAccess.DatabaseContext
{
    public class DataContext : DbContext, IDbContext
	{
        #region Constructors
        public DataContext() : base ("DataContext") { }
        public DataContext(string connectionString)
            : base(connectionString)
		{}
        #endregion

        #region Model Building
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ModelConfig.BuildModel(modelBuilder);
        }
        #endregion

        #region IDbContext Members

        public void DetectChanges(bool value)
        {
            throw new System.NotImplementedException();
        }

        //public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        //{
        //    _repositoriesCache = _repositoriesCache ?? new Hashtable();
        //    var entityType =  typeof(TEntity).Name;

        //    if (_repositoriesCache.ContainsKey(entityType))
        //    {
        //        return (IRepository<TEntity>)_repositoriesCache[entityType];
        //    }
            
        //    var concreteRepositoryType = Assembly.GetExecutingAssembly().ExportedTypes
        //        .SingleOrDefault(t => t.IsClass && typeof(Repository<TEntity>).IsAssignableFrom(t));

        //    var repoInstance = Activator.CreateInstance(concreteRepositoryType ?? typeof(Repository<>).MakeGenericType(typeof(TEntity)), this);
        //    _repositoriesCache.Add(entityType, repoInstance);
            
        //    return (IRepository<TEntity>)repoInstance;
        //}
        
        //public TRepository GetRepository<TRepository, TEntity>() 
        //    where TEntity : class 
        //    where TRepository: IRepository<TEntity>
        //{
        //    return (TRepository)this.GetRepository<TEntity>();
        //}

        public IDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return (IDbTransaction)Database.BeginTransaction(isolationLevel);
        }

        public void Refresh(object entity)
        {
            ((IObjectContextAdapter)this).ObjectContext.Refresh(RefreshMode.StoreWins, entity);
        }

        public Task RefreshAsync(object entity)
        {
            throw new NotImplementedException();
        }

        public bool TryGetObjectStateEntry(object entity, out IDbObjectStateEntry entry)
        {
            ObjectStateEntry stateEntry;
            if (((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out stateEntry))
            {
                entry = ToEntryState(stateEntry);
                return true;
            }

            entry = null;
            return false;

        }

        #endregion

        #region Private Members

        private Hashtable _repositoriesCache;

        private IDbObjectStateEntry ToEntryState(ObjectStateEntry entry)
        {
            if (entry != null)
                return new DbObjectStateEntry { Entity = entry.Entity, EntityKey = entry.EntityKey, State = ToObjectState(entry.State) };
            return null;
        }

        private DbObjectState ToObjectState(EntityState state)
        {
            switch (state)
            {
                case EntityState.Modified :
                    return DbObjectState.Modified;
                case EntityState.Unchanged :
                    return DbObjectState.Unchanged;
                default :
                    return DbObjectState.Unchanged;
            }
        }

        #endregion
    }
}
