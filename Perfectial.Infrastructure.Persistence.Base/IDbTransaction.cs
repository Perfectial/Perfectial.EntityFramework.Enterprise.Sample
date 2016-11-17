namespace Perfectial.Infrastructure.Persistence.Base
{
    public interface IDbTransaction
    {
        void Commit();
        void Rollback();
        void Dispose();
    }
}
