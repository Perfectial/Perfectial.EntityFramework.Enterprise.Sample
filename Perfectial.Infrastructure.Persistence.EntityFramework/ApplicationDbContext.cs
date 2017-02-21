namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    using IDbTransaction = Perfectial.Infrastructure.Persistence.Base.IDbTransaction;

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext()
            : base("ApplicationDbContext")
        {
        }

        public ApplicationDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public IDbSet<Student> Students { get; set; }
        public IDbSet<StudentAddress> StudentAddresses { get; set; }
        public IDbSet<Standard> Standards { get; set; }
        public IDbSet<Teacher> Teachers { get; set; }
        public IDbSet<Course> Courses { get; set; }

        public IDbSet<ToDoItem> ToDoItems { get; set; }
        public IDbSet<User> Users { get; set; }

        public IDbSet<Role> Roles { get; set; }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var dbContextTransaction = this.Database.BeginTransaction(isolationLevel);

            return new DbTransaction(dbContextTransaction);
        }

        public void AutoDetectChanges(bool value)
        {
            this.Configuration.AutoDetectChangesEnabled = value;
        }

        public bool TryGetObjectStateEntry(object entity, out IDbObjectStateEntry dbObjectStateEntry)
        {
            ObjectStateEntry objectStateEntry;
            dbObjectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out objectStateEntry) ?
                this.ToDbObjectStateEntry(objectStateEntry) :
                null;

            return dbObjectStateEntry != null;
        }

        public void Refresh(object entity)
        {
            ((IObjectContextAdapter)this).ObjectContext.Refresh(RefreshMode.StoreWins, entity);
        }

        public Task RefreshAsync(object entity)
        {
            return ((IObjectContextAdapter)this).ObjectContext.RefreshAsync(RefreshMode.StoreWins, entity);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ModelConfiguration.BuildModel(modelBuilder);
        }

        private IDbObjectStateEntry ToDbObjectStateEntry(ObjectStateEntry objectStateEntry)
        {
            IDbObjectStateEntry dbObjectStateEntry = null;
            if (objectStateEntry != null)
            {
                dbObjectStateEntry = new DbObjectStateEntry { Entity = objectStateEntry.Entity, EntityKey = objectStateEntry.EntityKey, State = this.ToDbObjectState(objectStateEntry.State) };
            }

            return dbObjectStateEntry;
        }

        private DbObjectState ToDbObjectState(EntityState state)
        {
            switch (state)
            {
                case EntityState.Unchanged:
                    return DbObjectState.Unchanged;
                case EntityState.Added:
                    return DbObjectState.Added;
                case EntityState.Modified:
                    return DbObjectState.Modified;
                case EntityState.Deleted:
                    return DbObjectState.Deleted;
                default:
                    return DbObjectState.Unchanged;
            }
        }
    }
}
