using Perfectial.DataAccess.DatabaseContext;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Contexts;

namespace Perfectial.Tests.Common
{
    public static class TestDatabaseCreator
    {
        private const string DEF_DB_Folder = "DataDirectory";

        static TestDatabaseCreator()
        {
            //set 'DataDirectory' as a DEF_DB_Folder
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DEF_DB_Folder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            AppDomain.CurrentDomain.SetData("DataDirectory", path);
        }

        public static DataContext Create<TMigrationsConfiguration>(string connectionStringName) where TMigrationsConfiguration : DbMigrationsConfiguration<DataContext>, new()
        {
            var context = new DataContext(connectionStringName);
            if (context.Database.Exists())
            {
                //set the database to SINGLE_USER so it can be dropped
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                                 "ALTER DATABASE [" + context.Database.Connection.Database + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");

                //drop the database
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                                 "USE master DROP DATABASE [" + context.Database.Connection.Database + "]");

                var startInfo = new ProcessStartInfo("sqllocaldb.exe") { UseShellExecute = false, Arguments = "stop v11.0" };
                var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                }
            }

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataContext, TMigrationsConfiguration>(connectionStringName));
            context.Database.Initialize(true);

            return context;
        }
    }
}
