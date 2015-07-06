using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Perfectial.DataAccess.Interfaces
{
    public interface IDbContext
    {
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken token);
        void DetectChanges(bool value);
        //IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        void Dispose();
        void Refresh(object entity);
        Task RefreshAsync(object entity);
        bool TryGetObjectStateEntry(object entity, out IDbObjectStateEntry entry);
    }
}
