namespace Perfectial.UnitTests.Application
{
    using System.Data.Entity;

    using Perfectial.Infrastructure.Persistence.EntityFramework;

    public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        public override void InitializeDatabase(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlCommand(
                TransactionalBehavior.DoNotEnsureTransaction,
                $"ALTER DATABASE {context.Database.Connection.Database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE");

            base.InitializeDatabase(context);
        }
    }
}
