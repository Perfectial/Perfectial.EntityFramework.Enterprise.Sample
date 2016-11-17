namespace Perfectial.UnitTests.Application
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Perfectial.Application.Model;
    using Perfectial.Application.Services;

    using Rhino.Mocks;
    using Is = Rhino.Mocks.Constraints.Is;

    [TestFixture]
    public class UserApplicationServiceTest : ApplicationServiceTestBase
    {
        private const int NumberOfUsersToCreate = 10;

        [Test]
        public async void ShouldGetAllUsers()
        {
            var users = this.CreateUsers(NumberOfUsersToCreate);
            var usersOutput = this.CreateUsersDto(NumberOfUsersToCreate);

            using (this.Repository.Record())
            {
                Expect.Call(this.DbContextScopeFactory.CreateReadOnly());
                Expect.Call(this.UserRepository.GetAllListAsync()).Return(Task.FromResult(users));
                Expect.Call(this.Mapper.Map<List<UserDto>>(null)).Constraints(Is.Equal(users)).Return(usersOutput).Repeat.Once();
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Getting Users"))).Repeat.Once();
                Expect.Call(() => this.Logger.Info(Arg.Text.Contains("Got Users"))).Repeat.Once();
            }

            using (this.Repository.Playback())
            {
                var userApplicationService = new UserApplicationService(
                    this.UserRepository,
                    this.DbContextScopeFactory,
                    this.Mapper,
                    this.Logger);
                var output = await userApplicationService.GetAllUsers();

                Assert.IsTrue(output.Users.Any());
                Assert.AreEqual(usersOutput.Count, output.Users.Count);
            }
        }
    }
}
