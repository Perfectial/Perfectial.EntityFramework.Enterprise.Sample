namespace Perfectial.Infrastructure.Persistence.Base
{
    public interface IDbObjectStateEntry
    {
        DbObjectState State { get; set; }

        object EntityKey { get; set; }
        object Entity { get; set; }
    }
}
