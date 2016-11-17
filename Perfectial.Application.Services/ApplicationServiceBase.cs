namespace Perfectial.Application.Services
{
    using AutoMapper;

    using Common.Logging;

    using Perfectial.Application.Services.Base;
    using Perfectial.Infrastructure.Persistence.Base;

    /// <summary>
    /// This class is used as a base class for application services.
    /// </summary>
    public abstract class ApplicationServiceBase : IApplicationServiceBase
    {
        protected ApplicationServiceBase(IDbContextScopeFactory dbContextScopeFactory, IMapper mapper, ILog logger)
        {
            this.DbContextScopeFactory = dbContextScopeFactory;
            this.Mapper = mapper;
            this.Logger = logger;
        }

        public IDbContextScopeFactory DbContextScopeFactory { get; set; }
        public IMapper Mapper { get; set; }
        public ILog Logger { get; set; }
    }
}
