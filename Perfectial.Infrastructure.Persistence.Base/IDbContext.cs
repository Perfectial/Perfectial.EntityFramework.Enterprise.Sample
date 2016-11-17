namespace Perfectial.Infrastructure.Persistence.Base
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDbContext
    {
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);

        void AutoDetectChanges(bool value);

        bool TryGetObjectStateEntry(object entity, out IDbObjectStateEntry entry);

        void Refresh(object entity);
        Task RefreshAsync(object entity);

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        void Dispose();
    }
}
