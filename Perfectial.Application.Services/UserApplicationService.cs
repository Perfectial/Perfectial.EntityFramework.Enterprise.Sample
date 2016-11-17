namespace Perfectial.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using AutoMapper;

    using Common.Logging;

    using Perfectial.Application.Model;
    using Perfectial.Application.Services.Base;
    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    public class UserApplicationService : ApplicationServiceBase, IUserApplicationService
    {
        private readonly IRepository<User, string> userRepository;

        public UserApplicationService(
            IRepository<User, string> userRepository,
            IDbContextScopeFactory dbContextScopeFactory, 
            IMapper mapper,
            ILog logger)
            : base(dbContextScopeFactory, mapper, logger)
        {
            this.userRepository = userRepository;
        }

        public async Task<GetUsersOutput> GetAllUsers()
        {
            this.Logger.Info("Getting Users");

            using (this.DbContextScopeFactory.CreateReadOnly())
            {
                var users = await this.userRepository.GetAllListAsync();
                var usersOutput = new GetUsersOutput { Users = this.Mapper.Map<List<UserDto>>(users) };

                this.Logger.Info($"Got Users : {usersOutput}");

                return usersOutput;
            }
        }
    }
}
