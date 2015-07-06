using LegalDecoder.Tests.Common.DatabaseInitializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Perfectial.Infrastructure.Services.Interfaces;
using Perfectial.Infrastructure.Services.Services;
using Perfectial.DataAccess.DatabaseContext;
using Perfectial.Tests.Common;
using Perfectial.UnitTests.Common;
using System.Linq;
using System.Threading.Tasks;
using Perfectial.DataAccess.Implementation;
using Perfectial.Common;
using Perfectial.Core.Domain.Enums;
using Perfectial.Core.Domain.Model;

namespace LegalDecoder.Tests.Business.Services
{
    [TestClass]
    public class UserServiceTest
    {
        private const string ConnectionStringName = "UserServiceTestContext";
        private static DataContext dataContext;
        private static IUserService _service;

        [ClassInitialize]
        public static void InitTestClass(TestContext context)
        {
            dataContext = TestDatabaseCreator.Create<UserDataInitializer>(ConnectionStringName);
            _service = new UserService(new DbContextScopeFactory(new MockSingleContextFactory(dataContext)));
        }

        [ClassCleanup]
        public static void CleanupTestClass()
        {
            dataContext.Dispose();
        }

        [TestMethod]
        public void SaveUsersAsync_Test()
        {
            UserDto admin = new UserDto()
            {
                Name = "John@mail.com",
                Password = "123456",
                UserType = UserType.Admin
            };
            var resultClient = _service.SaveUserAsync(admin).Result;

            UserDto moderator = new UserDto()
            {
                Name = "George@mail.com",
                Password = "123456",
                UserType = UserType.Moderator
            };
            var resultModerator = _service.SaveUserAsync(moderator).Result;

            Assert.IsTrue(resultClient);
            Assert.IsTrue(resultModerator);
        }

        [TestMethod]
        public void DeleteUserAsync_Test()
        {
            var user = dataContext.Set<User>().FirstOrDefault();
            var userId = user.Id;

            _service.DeleteAsync(userId).GetAwaiter().GetResult();

            dataContext.Refresh(user);

            Assert.IsTrue(user.Deleted);
        }
    }
}
