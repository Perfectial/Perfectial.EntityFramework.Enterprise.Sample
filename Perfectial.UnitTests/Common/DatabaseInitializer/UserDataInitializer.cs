using Perfectial.DataAccess.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LegalDecoder.Tests.Common.DatabaseInitializers
{
    public class UserDataInitializer : DbMigrationsConfiguration<DataContext>
    {
        private const string user = "user@mail.com";
        private const string password = "123456";
        private const string userSuper = "super@mail.com";
        private const string userCustomer = "customer@customer.com";
        private const string userCustomer1 = "customer1@customer1.com";

        public UserDataInitializer()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(DataContext context)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("UK-ua");
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("UK-ua");

            AddUsers(context);

        }

        private void AddUsers(DataContext context)
        {
            //TODO: add users to DB here
        }
    }
}
