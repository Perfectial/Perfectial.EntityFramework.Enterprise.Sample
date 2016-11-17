namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using System.Data.Entity;

    using Perfectial.Infrastructure.Persistence.Base;

    public class DbTransaction : IDbTransaction
    {
        private readonly DbContextTransaction dbContextTransaction;

        public DbTransaction(DbContextTransaction dbContextTransaction)
        {
            this.dbContextTransaction = dbContextTransaction;
        }

        public void Commit()
        {
            this.dbContextTransaction?.Commit();
        }

        public void Rollback()
        {
            this.dbContextTransaction?.Rollback();
        }

        public void Dispose()
        {
            this.dbContextTransaction?.Dispose();
        }
    }
}
