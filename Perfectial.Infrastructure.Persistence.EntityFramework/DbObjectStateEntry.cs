namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using Perfectial.Infrastructure.Persistence.Base;

    public class DbObjectStateEntry : IDbObjectStateEntry
    {
        public DbObjectState State { get; set; }

        public object EntityKey { get; set; }
        public object Entity { get; set; }
    }
}
